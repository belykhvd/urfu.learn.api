using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Common;
using Contracts.Types.Group;
using Contracts.Types.Group.ViewModel;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Repo;

namespace Core.Services
{
    public class GroupService : Repo<Group>, IGroupService
    {
        /* create table if not exists invite
           (
               secret uuid primary key,
               group_id uuid not null,
               email text not null,
               is_sent bool not null default false,
               is_accepted bool not null default false,
               student_id uuid
           );                                          */

        public GroupService(IConfiguration config) : base(config, PgSchema.group){}

        // TODO: update on conflict (group_id, email) when is_accepted = false
        public async Task InviteStudent(Guid groupId, string email)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"insert into {PgSchema.invite} (secret, group_id, email)
                       values (@Secret, @GroupId, @Email)",
                new
                {
                    Secret = Guid.NewGuid(),
                    groupId,
                    email
                }).ConfigureAwait(false);
        }

        public async Task<bool> AcceptInvite(Guid secret, Guid studentId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<bool>(
                $@"update {PgSchema.invite}
                       set is_accepted = true,
                           student_id = @StudentId
                       where secret = @Secret
                         and email = (select email from auth where user_id = @StudentId limit 1)
                       returning true",
                new
                {
                    secret,
                    studentId
                }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<GroupInviteItem>> GetInviteList()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QueryAsync<GroupInviteItem>(
                $@"select gr.data as group,
                          jsonb_agg(jsonb_build_object('email', inv.email, 'is_accepted', inv.is_accepted)) as invites
                       from {PgSchema.invite} inv
                       left join ""group"" gr
                         on inv.group_id = gr.id
                       group by group_id, data").ConfigureAwait(false);
        }

        public async Task<IEnumerable<StudentList>> GetStudentList(int year, int semester)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QueryAsync<StudentList>(
               $@"select jsonb_build_object('id', group_id, 'text', gi.name) as group,
                         jsonb_agg(jsonb_build_object('id', user_id, 'text', ui.fullname)) as users
	                  from {PgSchema.group_membership} gm
	                  join {PgSchema.group_index} gi
	                    on gm.group_id = gi.id
	                  join {PgSchema.user_index} ui
	                    on gm.user_id = ui.id
	                  where year = @Year
	                    and semester = @Semester
	                  group by group_id, gi.name", new { year, semester }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Group>> List()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return (await conn.QueryAsync<Group>(
                $@"select data from {PgSchema.group}").ConfigureAwait(false)).ToArray();
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
	                   from {PgSchema.group_membership} gm
	                   left join {PgSchema.user_index} pi on gm.user_id = pi.user_id
	                   where group_id = @GroupId", new { groupId }).ConfigureAwait(false)).ToArray());
        }

        public async Task<Result> Include(int year, int semester, Guid groupId, Guid userId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"insert into {PgSchema.group_membership} (year, semester, group_id, user_id)
                       values (@Year, @Semester, @GroupId, @UserId)",
                           new {year, semester, groupId, userId}).ConfigureAwait(false);

            return Result.Success;

            var conflicted = await conn.QuerySingleOrDefaultAsync<Guid?>(
                $@"insert into {PgSchema.group_membership} (year, semester, group_id, user_id)
                       values (@UserId, @GroupId)
                       on conflict (user_id) do update set group_id = {PgSchema.group_membership}.group_id
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
                $@"delete from {PgSchema.group_membership}
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

        public async Task<IEnumerable<StudentInvite>> GetStudents(Guid groupId)
        {
            throw new NotImplementedException();
        }

        protected override async Task SaveIndex(NpgsqlConnection conn, Guid id, Group data)
        {
            await conn.ExecuteAsync(
                @$"insert into {PgSchema.group_index} (id, is_deleted, name)
                       values (@Id, false, @Name)
                       on conflict (id) do update set name = @Name", new {id, data.Name}).ConfigureAwait(false);
        }

        protected override async Task DeleteIndex(NpgsqlConnection conn, Guid id)
        {
            await conn.ExecuteAsync(@$"delete from {PgSchema.group_index} where id = @Id", new {id}).ConfigureAwait(false);
        }
    }
}