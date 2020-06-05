using Contracts.Types.CheckSystem;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Repo;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JsJob
{
    public class JsCheckJob : PgRepo
    {
        private readonly FileRepo fileRepo;
        private readonly JsRunner jsRunner;

        public JsCheckJob(IConfiguration configuration, JsRunner jsRunner, FileRepo fileRepo)
             : base(configuration)
        {
            this.jsRunner = jsRunner;
            this.fileRepo = fileRepo;
        }

        public async Task Next()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            var checkTasks = await conn.QueryAsync<CheckTask>(
                $@"select json_build_object('taskId', task_id, 'solutionId', solution_id)
                       from js_queue
                       where not is_checked").ConfigureAwait(false);

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

                var testRunResult = jsRunner.Execute(solutionSource, testCase.Input);

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

            using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"insert into {PgSchema.js_check_result} (solution_id, passed, all_count, failed_number, stacktrace)
                       values (@SolutionId, @Passed, @All, @FailedNumber, @Stacktrace)",
                new
                {
                    solutionId,
                    passed = passedCount,
                    all = testCases.Length,
                    failedNumber = firstFailedNumber != -1 ? (int?) firstFailedNumber : null,
                    stacktrace = firstFailedNumber != -1 ? firstFailedError : null
                }).ConfigureAwait(false);
        }
    }
}