using Contracts.Types.CheckSystem;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Repo;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JsJob
{
    public class JsCheckJob : PgRepo
    {
        private const int DelayTimeout = 2000;
        
        private readonly JsRunner jsRunner;
        private readonly FileRepo fileRepo;

        public JsCheckJob(IConfiguration configuration, JsRunner jsRunner, FileRepo fileRepo)
             : base(configuration)
        {
            this.jsRunner = jsRunner;
            this.fileRepo = fileRepo;
        }

        public async Task Run(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await Task.Delay(DelayTimeout, cancellation).ConfigureAwait(false);
                await Next().ConfigureAwait(false);
            }
        }
        
        private async Task Next()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            var checkTasks = await conn.QueryAsync<CheckTask>(
                $@"select json_build_object('taskId', task_id, 'solutionId', solution_id)
                       from {PgSchema.js_queue}
                       where status = @Status", new {Status = CheckStatus.InQueue}).ConfigureAwait(false);

            foreach (var task in checkTasks)
            {
                await RunTests(task.TaskId, task.SolutionId).ConfigureAwait(false);
            }
        }

        private async Task RunTests(Guid taskId, Guid solutionId)
        {
            var solutionSource = await fileRepo.ReadSourceCode(solutionId).ConfigureAwait(false);
            var testCases = (await fileRepo.ReadTests(taskId).ConfigureAwait(false)).ToArray();

            var firstFailedNumber = -1;
            string firstFailedError = null;

            var passedCount = 0;
            for (var i = 0; i < testCases.Length; i++)
            {
                var testCase = testCases[i];

                var argumentArray = testCase.Input.Split().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                var testRunResult = jsRunner.Execute(solutionSource, argumentArray);

                if (testRunResult.IsSuccess)
                {
                    if (testRunResult.Output == testCase.CorrectOutput)
                    {
                        passedCount++;
                    }
                    else if (firstFailedNumber == -1)
                    {
                        firstFailedNumber = i + 1;
                        firstFailedError = "Wrong answer";
                    }
                }
                else if (firstFailedNumber == -1)
                {
                    firstFailedNumber = i + 1;
                    firstFailedError = testRunResult.ErrorMessage;
                }
            }

            await MarkFinished(taskId, solutionId, passedCount, testCases.Length, firstFailedNumber, firstFailedError).ConfigureAwait(false);
        }

        private async Task MarkFinished(
            Guid taskId,
            Guid solutionId,
            int passedCount,
            int allCount,
            int failedNumber,
            string failedStacktrace)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"insert into {PgSchema.js_check_result} (solution_id, passed, all_count, failed_number, stacktrace)
                       values (@SolutionId, @Passed, @All, @FailedNumber, @Stacktrace)
                       on conflict (solution_id) do update set passed = @Passed,
                                                               all_count = @All,
                                                               failed_number = @FailedNumber,
                                                               stacktrace = @Stacktrace",
                new
                {
                    solutionId,
                    passed = passedCount,
                    all = allCount,
                    failedNumber = failedNumber != -1 ? (int?) failedNumber : null,
                    stacktrace = failedNumber != -1 ? failedStacktrace : null
                }).ConfigureAwait(false);

            await conn.ExecuteAsync(
                $@"update {PgSchema.js_queue}
                       set status = @Status
                       where task_id = @TaskId
                         and solution_id = @SolutionId", new {taskId, solutionId, Status = CheckStatus.Finished}).ConfigureAwait(false);
        }
    }
}