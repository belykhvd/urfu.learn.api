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

        Task<OperationStatus> Include(Guid groupId, Guid userId);
        Task<OperationStatus> Exclude(Guid groupId, Guid userId);
        Task<OperationStatus<StudentDescription[]>> GetMembers(Guid groupId);

        #endregion

        #region SEARCH

        Task<GroupDescription[]> Search(string prefix, int? limit);

        #endregion
    }
}