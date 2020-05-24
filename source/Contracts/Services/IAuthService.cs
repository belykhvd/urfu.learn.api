using System.Threading.Tasks;
using Contracts.Types.Auth;
using Contracts.Types.User;

namespace Contracts.Services
{
    public interface IAuthService
    {
        Task<AuthResult> SignUp(RegistrationData registrationData);
        Task<AuthResult> Authorize(AuthData authData);
    }
}