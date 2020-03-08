using System;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Common;
using Contracts.Types.User;

namespace Contracts.Services
{
    public interface IUserService : ICrudRepo<Profile>
    {
        #region Profile

        Task<Profile> GetProfile(Guid userId);
        Task SaveProfile(Guid userId, Profile profile);

        #endregion

        #region Profile photo

        Task<string> GetProfilePhoto(Guid userId);
        Task SaveProfilePhoto(Guid userId, string photoBase64);
        Task DeleteProfilePhoto(Guid userId);

        #endregion
    }
}