using System;
using System.Threading.Tasks;
using Contracts.Types.User;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Repo;

namespace Core.Repo
{
    public class ProfileRepo : Repo<Profile>
    {
        public ProfileRepo(IConfiguration config) : base(config, PgSchema.user_profile)
        {
        }

        protected override async Task SaveIndex(NpgsqlConnection conn, Guid id, Profile data)
        {
            await conn.ExecuteAsync(
                @$"insert into {PgSchema.user_index} (id, fio)
                       values (@Id, @Fio)
                       on conflict (id) do update set fio = @Fio", new {id, fio = data.Fio()}).ConfigureAwait(false);
        }
    }
}