using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Common;
using Contracts.Types.Group;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Services
{
    public class GroupService : PgRepo, IGroupService
    {
        protected GroupService(IConfiguration config) : base(config)
        {
        }

        public Task<GroupSummary[]> SearchGroup(string prefix, int? limit)
        {
            throw new NotImplementedException();
        }

        public async Task<MemberSummary[]> SelectMemberSummaries(Guid groupId, SelectParameters parameters)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.QueryAsync(
                @"select user_id
                      from ").ConfigureAwait(false);
            
            throw new NotImplementedException();
        }
    }
}