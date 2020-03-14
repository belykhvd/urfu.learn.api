using System;
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

        public async Task<Result> Include(Guid groupId, Guid userId) => await Try(IncludeInternal(groupId, userId), $"{nameof(Group)}.{nameof(Include)}").ConfigureAwait(false);
        public async Task<Result> Exclude(Guid groupId, Guid userId) => await Try(ExcludeInternal(groupId, userId), $"{nameof(Group)}.{nameof(Exclude)}").ConfigureAwait(false);
        public async Task<Result<StudentDescription[]>> GetMembers(Guid groupId) => await Try(GetMembersInternal(groupId), $"{nameof(Group)}.{nameof(GetMembers)}").ConfigureAwait(false);

        private async Task<Result> IncludeInternal(Guid groupId, Guid userId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);

            var conflicted = await conn.QuerySingleOrDefaultAsync<Guid?>(
                $@"insert into {PgSchema.GroupMembership} (user_id, group_id)
                       values (@UserId, @GroupId)
                       on conflict (user_id) do update set group_id = {PgSchema.GroupMembership}.group_id
                       returning group_id", new {groupId, userId}).ConfigureAwait(false);

            return conflicted == null || conflicted.Value == groupId
                ? Result.Success
                : Result.Fail(OperationStatusCode.Conflict,
                    "Студент уже числится в другой академической группе. Сначала исключите его из нее.");
        }

        private async Task<Result> ExcludeInternal(Guid groupId, Guid userId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                $@"delete from {PgSchema.GroupMembership}
                         where user_id = @UserId
                           and group_id = @GroupId", new {userId, groupId}).ConfigureAwait(false);
            return Result.Success;
        }

        private async Task<Result<StudentDescription[]>> GetMembersInternal(Guid groupId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return Result<StudentDescription[]>.Success(
                (await conn.QueryAsync<StudentDescription>(
                $@"select gm.user_id, pi.fullname
	                   from {PgSchema.GroupMembership} gm
	                   left join {PgSchema.ProfileIndex} pi on gm.user_id = pi.user_id
	                   where group_id = @GroupId", new {groupId}).ConfigureAwait(false)).ToArray());
        }

        public Task<GroupDescription[]> Search(string prefix, int? limit)
        {
            throw new NotImplementedException();
        }

        protected async Task<Result> Try(Task<Result> task, string path)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                var status = await task.ConfigureAwait(false);

                sw.Stop();
                Console.WriteLine($"{path} {sw.Elapsed}");
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);  // logging
                return Result.Fail(OperationStatusCode.InternalServerError);
            }
        }

        protected async Task<Result<T>> Try<T>(Task<Result<T>> task, string path)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                var status = await task.ConfigureAwait(false);

                sw.Stop();
                Console.WriteLine($"{path} {sw.Elapsed}");
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);  // logging
                return Result<T>.Fail(OperationStatusCode.InternalServerError);
            }
        }
    }
}