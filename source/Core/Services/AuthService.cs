using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
using Contracts.Types.Common;
using Contracts.Types.User;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core.Services
{
    // TODO: maybe use JWT?
    public class AuthService : PgRepo, IAuthService
    {
        public AuthService(IConfiguration config) : base(config)
        {
        }

       /*
        * TODO: 0. Positive cases (valid registrationData)
        * TODO: 1. Validation
        * TODO: 2. postgres: user already exists
        * TODO: 3. postgres: save in transaction
        */
        public async Task<Result<Guid>> SignUp(RegistrationData registrationData)
        {
            var profile = new Profile
            {
                Surname = registrationData.Surname,
                FirstName = registrationData.FirstName,
                SecondName = registrationData.SecondName,
                Group = registrationData.Group
            };

            var authData = new AuthData
            {
                Email = registrationData.Email,
                Password = registrationData.Password
            };

            /*
             * Validation:
             * 1. null
             * 2. user email already registered
             * 3. weak password
             * 4. group not found, other about groups
             * 5. restricted characters in email/password/other
             *
             * errorMessage = validationMessage
             */

            var userId = Guid.NewGuid();

            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.ExecuteAsync(
                @"insert into profile (user_id, data) values (@UserId, @Profile::jsonb)",
                new {userId, profile}).ConfigureAwait(false);

            await conn.ExecuteAsync(
                @"insert into auth_data (login, password_hash, user_id) values (@Email, digest(@Password, 'sha256'), @UserId)",
                new {authData.Email, authData.Password, userId}).ConfigureAwait(false);

            return new Result<Guid> {IsSuccessful = true, Value = userId};
        }
        
        public async Task<Result<string>> SignIn(AuthData authData)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            var userId = await conn.QuerySingleOrDefaultAsync<Guid?>(
                @"select user_id
                      from auth_data
                      where email = @Email
                        and password_hash = digest(@Password, 'sha256')
                        limit 1", new {authData.Email, authData.Password}).ConfigureAwait(false);

            if (!userId.HasValue)
            {
                return new Result<string>
                {
                    IsSuccessful = false,
                    ErrorMessage = "User not found"
                };
            }

            var token = Guid.NewGuid().ToString();
            await conn.ExecuteAsync(
                @"insert into auth_session (user_id, token)
                      values (@UserId, @Token)
                      on conflict do update set token = @Token", new {UserId = userId.Value, token}).ConfigureAwait(false);

            return new Result<string>
            {
                IsSuccessful = true,
                Value = token
            };
        }

        public async Task<bool> SignOut(string token)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            var isDeleted = await conn.QuerySingleOrDefaultAsync<bool?>(
                @"delete from auth_session
                      where token = @Token
                      returning true", new {token}).ConfigureAwait(false);

            return isDeleted.HasValue;
        }

        public async Task<Guid?> Authenticate(string token)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<Guid?>(
                @"select user_id
                      from auth_session
                      where token = @Token
                      limit 1", new {token}).ConfigureAwait(false);
        }
    }
}