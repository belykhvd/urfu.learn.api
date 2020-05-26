using System;
using System.Threading.Tasks;
using Contracts.Types.Auth;

namespace Contracts.Services
{
    public interface IAuthService
    {
        Task<AuthResult> SignUp(RegistrationData registrationData);
        Task<AuthResult> Authorize(AuthData authData);
        Task<bool> ChangePassword(Guid userId, PasswordData passwordData);
    }
}