using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Group;
using Contracts.Types.Group.ViewModel;

namespace Contracts.Services
{
    public interface IGroupService : IRepo<Group>
    {
        Task GrantAccess(Guid groupId, Guid[] courseIds);
        Task RevokeAccess(Guid groupId, Guid[] courseIds);

        Task<bool> InviteStudent(Guid groupId, string email, bool sendInvite = true);
        Task ExcludeStudent(Guid groupId, string email);

        Task<bool> AcceptInvite(Guid secret, Guid studentId);

        Task<IEnumerable<GroupItem>> GetUsers();
        Task<IEnumerable<GroupInviteItem>> GetInviteList();
        

        Task<IEnumerable<Group>> List();
    }
}