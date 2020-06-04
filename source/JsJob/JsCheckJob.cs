using Dapper;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace JsJob
{
    public class JsCheckJob
    {
        private readonly string connectionString;

        public async Task ProcessNext()
        {
            await using var conn = new NpgsqlConnection(connectionString);
            var unprocessedSolutions = await conn.QueryAsync(
                $@"select solution_id
                       from js_queue
                       where not is_checked").ConfigureAwait(false);

            foreach (var solutionId in unprocessedSolutions)
            {

            }
        }

        private async Task RunTests(Guid taskId, Guid solutionId)
        {
             
        }
    }
}