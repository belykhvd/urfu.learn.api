using System;
using System.Threading.Tasks;
using Contracts.Types.Common;
using Contracts.Types.Group;

namespace Contracts.Services
{
    public interface IGroupService
    {
        Task<GroupSummary[]> SearchGroup(string prefix, int? limit);
        Task<MemberSummary[]> SelectMemberSummaries(Guid groupId, SelectParameters parameters);
    }
}