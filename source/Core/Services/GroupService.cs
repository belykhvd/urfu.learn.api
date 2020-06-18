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

        public async Task RevokeAccess(Guid groupId, Guid[] courseIds)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"delete from {PgSchema.course_access}
                       where group_id = @GroupId
                         and course_id =any (@CourseIds)",
                new
                {
                    groupId,
                    courseIds
                }).ConfigureAwait(false);
        }

        public async Task<bool> InviteStudent(Guid groupId, string email, bool sendInvite = true)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);

            if (!sendInvite)
            {
                return await conn.QuerySingleOrDefaultAsync<bool>(
                    $@"insert into {PgSchema.invite} (secret, group_id, email, is_sent)
                           values (@Secret, @GroupId, @Email, true)
                       on conflict (group_id, email) do nothing
                       returning true",
                    new
                    {
                        Secret = Guid.NewGuid(),
                        groupId,
                        email
                    }).ConfigureAwait(false);
            }
            
            return await conn.QuerySingleOrDefaultAsync<bool>(
                $@"insert into {PgSchema.invite} (secret, group_id, email)
                       values (@Secret, @GroupId, @Email)
                       on conflict (group_id, email) do nothing
                       returning true",
                new
                {
                    Secret = Guid.NewGuid(),
                    groupId,
                    email
                }).ConfigureAwait(false);
        }

        public async Task ExcludeStudent(Guid groupId, string email)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"delete from {PgSchema.invite}
                       where group_id = @GroupId
                         and email = @Email",
                new
                {
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
                                'isAccepted', inv.is_accepted,
                                'studentId', inv.student_id,
                                'name', ui.fio 
                                )
                       from {PgSchema.invite} inv
                       left join {PgSchema.user_index} ui
                         on inv.student_id is not null
                         and inv.student_id = ui.id
                       where inv.group_id = @GroupId", new {GroupId = group.Id}).ConfigureAwait(false)).ToArray();

                item.CourseList = (await conn.QueryAsync<Link>(
                    $@"select jsonb_build_object('id', ci.id, 'name', ci.name) 
                           from {PgSchema.course_access} ca
                           left join {PgSchema.course_index} ci
                             on ca.course_id = ci.id
                           where group_id = @GroupId
                             and ci.id is not null", new {GroupId = item.Id}).ConfigureAwait(false)).ToArray();

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

            return groupItems.OrderBy(x =>
            {
                if (x.Id == Guid.Empty) return " ";
                if (x.Id.ToString("N") == "00000000000000000000000000000001") return "  ";
                return x.Name;
            });
        }

        public async Task<IEnumerable<Group>> List()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return (await conn.QueryAsync<Group>(
                $@"select data from {PgSchema.group}").ConfigureAwait(false)).ToArray();
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

        public new async Task Delete(Guid id)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);
            await using (transaction)
            {
                await conn.ExecuteAsync($@"delete from {PgSchema.group} where id = @Id", new {id}).ConfigureAwait(false);
                await conn.ExecuteAsync($@"delete from {PgSchema.group_index} where id = @Id", new {id}).ConfigureAwait(false);
                await conn.ExecuteAsync($@"delete from {PgSchema.course_access} where group_id = @Id", new {id}).ConfigureAwait(false);
                await conn.ExecuteAsync($@"delete from {PgSchema.invite} where group_id = @Id", new {id}).ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
        }
    }
}