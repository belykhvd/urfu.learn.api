using Contracts.Repo;
using Contracts.Types.Challenge;

namespace Contracts.Services
{
    public interface IChallengeService : ICrudRepo<Challenge>
    {
    }
}