using System;
using System.Threading.Tasks;
using Contracts.Types.Auth;
using Contracts.Types.User;

namespace Contracts.Services
{
    public interface IAuthService
    {
        Task<Guid> SignUp(RegistrationData registrationData);
        Task<Guid?> TryGetUserId(AuthData authData);

        //Task<Result<Guid>> SignUp(RegistrationData registrationData);

    }
}