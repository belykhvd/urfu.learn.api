using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
using Core.Repo;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Repo;

namespace Core.Services
{
    public class AuthService : PgRepo, IAuthService
    {
        private readonly ProfileRepo profileRepo;

        public AuthService(IConfiguration config, ProfileRepo profileRepo) : base(config)
        {
            this.profileRepo = profileRepo;
        }

        public async Task<AuthResult> SignUp(RegistrationData registrationData)
        {
            var userId = Guid.NewGuid();

            var profile = registrationData.Profile;
            var authData = registrationData.AuthData;

            await using var conn = new NpgsqlConnection(ConnectionString);

            await conn.ExecuteAsync(
                $@"insert into {PgSchema.auth} (email, password_hash, user_id, role)
                      values (@Email, digest(@Password, 'sha256'), @UserId, @Role)",
                new {authData.Email, authData.Password, userId, authData.Role}).ConfigureAwait(false);

            await profileRepo.Save(userId, profile).ConfigureAwait(false);

            return new AuthResult
            {
                UserId = userId,
                Role = authData.Role,
                Fio = profile.Fio()
            };
        }

        public async Task<AuthResult> Authorize(AuthData authData)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<AuthResult>(
                $@"select json_build_object('userid', user_id, 'role', role, 'fio', fio)
                      from {PgSchema.auth} au
                      join {PgSchema.user_index} ui
                        on au.user_id = ui.id
                      where email = @Email
                        and password_hash = digest(@Password, 'sha256')
                      limit 1", new {authData.Email, authData.Password}).ConfigureAwait(false);
        }

        public async Task<bool> ChangePassword(Guid userId, PasswordData passwordData)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            return await conn.QuerySingleOrDefaultAsync<bool>(
                $@"update {PgSchema.auth}
                      set password_hash = digest(@New, 'sha256')
                      where user_id = @UserId
                        and password_hash = digest(@Current, 'sha256')
                      returning true",
                new
                {
                    userId,
                    passwordData.New,
                    passwordData.Current
                }).ConfigureAwait(false);
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