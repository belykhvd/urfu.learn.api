using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Solution;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Services
{
    public class SolutionService : PgRepo, ISolutionService
    {
        public SolutionService(IConfiguration config) : base(config)
        {
        }

        public async Task Upload(Solution solution)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                @"insert into solution (id, data) values (@Id, @Data::jsonb)",
                new {solution.Id, Data = solution}).ConfigureAwait(false);
        }

        public async Task<byte[]> Download(Guid solutionId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            var solution = await conn.QuerySingleOrDefaultAsync<Solution>(
                @"select data from solution where id = @Id limit 1",
                new {Id = solutionId}).ConfigureAwait(false);

            return solution?.ContentBase64 != null
                ? Convert.FromBase64String(solution.ContentBase64)
                : null;
        }

        public async Task<IEnumerable<SolutionStudentSummary>> SelectStudentSummaries(Guid taskId, int lastLoadedIndex, int limit)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.QueryAsync(
                @"select data from solution where id = @").ConfigureAwait(false);

            return null;
        }

        public async Task RateProgress(SolutionProgress progress)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                @"insert into solution_rate (id, data) values (@Id, @Data)",
                new {progress.SolutionId, Data = progress}).ConfigureAwait(false);
        }
    }
}