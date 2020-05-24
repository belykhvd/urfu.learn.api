using System;
using System.Threading.Tasks;
using Contracts.Types.User;

namespace Contracts.Services
{
    public interface IUserService
    {
        Task<Profile> GetProfile(Guid userId);
        Task SaveProfile(Profile profile);
    }
}