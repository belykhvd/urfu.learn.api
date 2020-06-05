using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.User;
using Core.Repo;
using Microsoft.Extensions.Configuration;
using Repo;

namespace Core.Services
{
    public class UserService : PgRepo, IUserService
    {
        private readonly ProfileRepo profileRepo;

        public UserService(IConfiguration configuration, ProfileRepo profileRepo) : base(configuration)
        {
            this.profileRepo = profileRepo;
        }

        public async Task<Profile> GetProfile(Guid userId)
            => await profileRepo.Get(userId).ConfigureAwait(false);

        public async Task SaveProfile(Guid userId, Profile profile)
            => await profileRepo.Save(userId, profile).ConfigureAwait(false);
    }
}