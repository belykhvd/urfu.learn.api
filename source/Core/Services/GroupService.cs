using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Common;
using Contracts.Types.Group;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Services
{
    public class GroupService : CrudRepo<Group>, IGroupService
    {
        public GroupService(IConfiguration config) : base(config, PgSchema.Group){}

        public async Task<IEnumerable<StudentList>> GetStudentList(int year, int semester)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QueryAsync<StudentList>(
               $@"select jsonb_build_object('id', group_id, 'text', gi.name) as group,
                         jsonb_agg(jsonb_build_object('id', user_id, 'text', ui.fullname)) as users
	                  from {PgSchema.GroupMembership} gm
	                  join {PgSchema.GroupIndex} gi
	                    on gm.group_id = gi.id
	                  join {PgSchema.UserIndex} ui
	                    on gm.user_id = ui.id
	                  where year = @Year
	                    and semester = @Semester
	                  group by group_id, gi.name", new { year, semester }).ConfigureAwait(false);
        }

        public async Task<GroupLink[]> List()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return (await conn.QueryAsync<GroupLink>(
                $@"select data from {PgSchema.Group}").ConfigureAwait(false)).ToArray();
        }


        public async Task<Result<StudentDescription[]>> ListMembers(int year, int semester, Guid groupId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return Result<StudentDescription[]>.Success(
                (await conn.QueryAsync<StudentDescription>(
                    $@"select ui.*
	                       from group_membership gm
                           join user_index ui
	                         on gm.user_id = ui.id
	                       where year = 2020
	                         and semester = 8
	                         and group_id = '00000000-0000-0000-0000-000000000001';").ConfigureAwait(false)).ToArray());
            
            return Result<StudentDescription[]>.Success(
                (await conn.QueryAsync<StudentDescription>(
                $@"select gm.user_id, pi.fullname
	                   from {PgSchema.GroupMembership} gm
	                   left join {PgSchema.ProfileIndex} pi on gm.user_id = pi.user_id
	                   where group_id = @GroupId", new { groupId }).ConfigureAwait(false)).ToArray());
        }

        public async Task<Result> Include(int year, int semester, Guid groupId, Guid userId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"insert into {PgSchema.GroupMembership} (year, semester, group_id, user_id)
                       values (@Year, @Semester, @GroupId, @UserId)",
                           new {year, semester, groupId, userId}).ConfigureAwait(false);

            return Result.Success;

            var conflicted = await conn.QuerySingleOrDefaultAsync<Guid?>(
                $@"insert into {PgSchema.GroupMembership} (year, semester, group_id, user_id)
                       values (@UserId, @GroupId)
                       on conflict (user_id) do update set group_id = {PgSchema.GroupMembership}.group_id
                       returning group_id", new {groupId, userId}).ConfigureAwait(false);

            return conflicted == null || conflicted.Value == groupId
                ? Result.Success
                : Result.Fail(OperationStatusCode.Conflict,
                    "Студент уже числится в другой академической группе. Сначала исключите его из нее.");
        }

        public async Task<Result> Exclude(int year, int semester, Guid groupId, Guid userId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"delete from {PgSchema.GroupMembership}
                       where year = @Year
                         and semester = @Semester
                         and group_id = @GroupId
                         and user_id = @UserId", new {year, semester, groupId, userId}).ConfigureAwait(false);
            return Result.Success;
        }

        
        public Task<GroupLink[]> Search(string prefix, int? limit)
        {
            throw new NotImplementedException();
        }      
    }
}