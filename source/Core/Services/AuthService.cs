using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
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

        public async Task<Guid> SignUp(RegistrationData registrationData)
        {
            var userId = Guid.NewGuid();

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

            await using var conn = new NpgsqlConnection(ConnectionString);

            await conn.ExecuteAsync(
                @"insert into auth (email, password_hash, user_id)
                      values (@Email, digest(@Password, 'sha256'), @UserId)",
                new { authData.Email, authData.Password, UserId = userId }).ConfigureAwait(false);

            await conn.ExecuteAsync(
                @"insert into user_profile (id, data)
                      values (@Id, @Profile::jsonb)", new { Id = userId, profile }).ConfigureAwait(false);

            return userId;
        }

        public async Task<Guid?> TryGetUserId(AuthData authData)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<Guid?>(
                @"select user_id
                      from auth
                      where email = @Email
                        and password_hash = digest(@Password, 'sha256')
                        limit 1", new { authData.Email, authData.Password }).ConfigureAwait(false);
        }

        //public async Task<Result<Guid>> SignUp_(RegistrationData registrationData)
        //{            
        //    var profile = new Profile
        //    {
        //        Surname = registrationData.Surname,
        //        FirstName = registrationData.FirstName,
        //        SecondName = registrationData.SecondName,
        //        Group = registrationData.Group
        //    };

        //    var authData = new AuthData
        //    {
        //        Email = registrationData.Email,
        //        Password = registrationData.Password
        //    };

        //    try
        //    {
        //        //profile.InitAsFresh();

        //        await using var conn = new NpgsqlConnection(ConnectionString);
        //        await conn.OpenAsync().ConfigureAwait(false);
        //        await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);

        //        //await conn.ExecuteAsync(
        //        //    @"insert into auth (email, password_hash, user_id)
        //        //          values (@Email, digest(@Password, 'sha256'), @UserId)",
        //        //    new {authData.Email, authData.Password, UserId = profile.Id}).ConfigureAwait(false);

        //        await conn.ExecuteAsync(
        //            @"insert into user_profile (id, data)
        //                  values (@Id, @Profile::jsonb)", new {profile.Id, profile}).ConfigureAwait(false);

        //        await transaction.CommitAsync().ConfigureAwait(false);

        //        return Result<Guid>.Success(profile.Id);
        //    }
        //    catch (PostgresException e) when (e.SqlState == "23505")
        //    {
        //        return Result<Guid>.Fail(OperationStatusCode.Conflict, "User with such email already exists");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);  // TODO logging
        //        return Result<Guid>.Fail(OperationStatusCode.InternalServerError);
        //    }
        //}
    }
}