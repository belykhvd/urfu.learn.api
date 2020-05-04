using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.User;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Services
{
    public class UserService : Repo<Profile>, IUserService
    {
        public UserService(IConfiguration config) : base(config, "profile")
        {
        }

        #region Profile photo

        public async Task<string> GetProfilePhoto(Guid userId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<string>(
                @"select photo_base64 from profile_photo where user_id = @UserId limit 1", new {userId}).ConfigureAwait(false);
        }

        public async Task SaveProfilePhoto(Guid userId, string photoBase64)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                @"insert into profile_photo (user_id, photo_base64) values (@UserId, @PhotoBase64)
                      on conflict do update set photo_base64 = @PhotoBase64", new {userId, photoBase64}).ConfigureAwait(false);
        }

        public async Task DeleteProfilePhoto(Guid userId)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                @"delete from profile_photo where userId = @UserId", new {userId}).ConfigureAwait(false);
        }

        #endregion
    }
}