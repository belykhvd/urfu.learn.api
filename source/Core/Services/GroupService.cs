using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
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

        public async Task GrantAccess(Guid groupId, Guid[] courseIds)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            foreach (var courseId in courseIds)
            {
                await conn.ExecuteAsync(
                    $@"insert into {PgSchema.course_access} (group_id, course_id)
                           values (@GroupId, @CourseId)",
                    new
                    {
                        groupId,
                        courseId
                    }).ConfigureAwait(false);
            }
        }

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
            var groups = await conn.QueryAsync<Group>(
                $@"select data
                       from {PgSchema.group}
                       order by data->>'Name'").ConfigureAwait(false);

            var items = new List<GroupInviteItem>();
            foreach (var group in groups)
            {
                var item = new GroupInviteItem
                {
                    Id = group.Id,
                    Name = group.Name
                };

                item.InviteList = (await conn.QueryAsync<StudentInvite>(
                    $@"select jsonb_build_object(
                                'email', inv.email,
                                'isAccepted', inv.is_accepted)
                       from {PgSchema.invite} inv
                       where inv.group_id = @GroupId", new {GroupId = group.Id}).ConfigureAwait(false)).ToArray();

                item.CourseList = (await conn.QueryAsync<Link>(
                    $@"select jsonb_build_object('id', ci.id, 'name', ci.name) 
                           from {PgSchema.course_access} ca
                           left join {PgSchema.course_index} ci
                             on ca.course_id = ci.id
                           where group_id = @GroupId", new {GroupId = item.Id}).ConfigureAwait(false)).ToArray();

                items.Add(item);
            }

            return items;
        }

        public async Task<IEnumerable<GroupItem>> GetUsers()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            var groups = await conn.QueryAsync<Group>(
                $@"select data
                       from {PgSchema.group}
                       order by data->>'Name'").ConfigureAwait(false);

            var groupItems = new List<GroupItem>();
            foreach (var group in groups)
            {
                var item = new GroupItem
                {
                    Id = group.Id,
                    Name = group.Name,
                    Year = group.Year
                };

                item.StudentList = (await conn.QueryAsync<StudentItem>(
                    $@"select jsonb_build_object('userId', ui.id, 'studentName', ui.fio)
                           from invite inv
                           left join user_index ui
                             on inv.student_id = ui.id
                           where is_accepted
                             and group_id = @GroupId
                           order by ui.fio", new {GroupId = item.Id}).ConfigureAwait(false))
                    .ToArray();

                groupItems.Add(item);
            }
            
            // var groupItems_ = (await conn.QueryAsync<GroupItem>(
            //     $@"select gr.data || jsonb_build_object('studentList', jsonb_agg(
            // case when inv.student_id is not null then jsonb_build_object('userId', inv.student_id, 'studentName', ui.fio)
            //   else null end
            // )       
            //     )
            //            from group_index gi
            //            left join {PgSchema.group} gr
            // on gi.id = gr.id
            // left join {PgSchema.invite} inv
            // on gi.id = inv.group_id
            // left join {PgSchema.user_index} ui
            //   on inv.student_id = ui.id
            // where is_accepted
            //    or inv.group_id is null
            // group by group_id, data").ConfigureAwait(false)).ToArray();
            //
            // foreach (var item in groupItems)
            // {
            //     item.StudentList = item.StudentList
            //         .Where(x => x != null)
            //         .OrderBy(x => x.StudentName)
            //         .ToArray();
            // }

            var admins = await conn.QueryAsync<StudentItem>(
                $@"select jsonb_build_object('userId', au.user_id, 'studentName', ui.fio)
                       from {PgSchema.auth} au
                       left join {PgSchema.user_index} ui
                         on au.user_id = ui.id
                       where role = @AdminRole
                       order by ui.fio", new {AdminRole = UserRole.Admin}).ConfigureAwait(false);
            
            var adminGroupItem = new GroupItem
            {
                Name = "Преподаватели",
                StudentList = admins.ToArray() 
            };

            return new[] {adminGroupItem}.Concat(groupItems.OrderBy(x => x.Name));
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