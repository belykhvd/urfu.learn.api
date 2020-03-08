using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
using Contracts.Types.Common;
using Contracts.Types.User;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Services
{
    public class AuthService : PgRepo, IAuthService
    {
        public AuthService(IConfiguration config) : base(config)
        {
        }

        public async Task<OperationStatus<Guid>> SignUp(RegistrationData registrationData)
        {
            // TODO
            var profile = new Profile {Surname = registrationData.Surname, FirstName = registrationData.FirstName, SecondName = registrationData.SecondName, Group = registrationData.Group};
            var authData = new AuthData {Email = registrationData.Email, Password = registrationData.Password};

            try
            {
                profile.InitAsFresh();

                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync().ConfigureAwait(false);
                await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);

                await conn.ExecuteAsync(
                    @"insert into auth_data (email, password_hash, user_id)
                          values (@Email, digest(@Password, 'sha256'), @UserId)",
                    new {authData.Email, authData.Password, UserId = profile.Id}).ConfigureAwait(false);

                await conn.ExecuteAsync(
                    @"insert into profile (id, deleted, version, data)
                          values (@Id, false, 0, @Profile::jsonb)", new {profile.Id, profile}).ConfigureAwait(false);

                await transaction.CommitAsync().ConfigureAwait(false);

                return OperationStatus<Guid>.Success(profile.Id);
            }
            catch (PostgresException e) when (e.SqlState == "23505")
            {
                return OperationStatus<Guid>.Fail(OperationStatusCode.Conflict, "User with such email already exists");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);  // TODO logging
                return OperationStatus<Guid>.Fail(OperationStatusCode.InternalServerError);
            }
        }

        public async Task<Guid?> TryGetUserId(AuthData authData)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<Guid?>(
                @"select user_id
                      from auth_data
                      where email = @Email
                        and password_hash = digest(@Password, 'sha256')
                        limit 1", new {authData.Email, authData.Password}).ConfigureAwait(false);
        }
    }
}