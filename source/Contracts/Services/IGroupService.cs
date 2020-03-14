using System;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Common;
using Contracts.Types.Group;

namespace Contracts.Services
{
    public interface IGroupService : ICrudRepo<Group>
    {
        #region MEMBERSHIP

        Task<Result> Include(Guid groupId, Guid userId);
        Task<Result> Exclude(Guid groupId, Guid userId);
        Task<Result<StudentDescription[]>> GetMembers(Guid groupId);

        #endregion

        #region SEARCH

        Task<GroupDescription[]> Search(string prefix, int? limit);

        #endregion
    }
}