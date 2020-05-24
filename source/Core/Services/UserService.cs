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
    public class UserService : PgRepo, IUserService
    {
        public UserService(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<Profile> GetProfile(Guid userId)
        {
            using var conn = new NpgsqlConnection(ConnectionString);

            await conn.QuerySingleOrDefaultAsync<Profile>($@"").ConfigureAwait(false);
        }

        public Task SaveProfile(Profile profile)
        {
            throw new NotImplementedException();
        }
    }
}