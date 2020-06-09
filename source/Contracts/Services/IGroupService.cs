using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Common;
using Contracts.Types.Group;
using Contracts.Types.Group.ViewModel;

namespace Contracts.Services
{
    public interface IGroupService : IRepo<Group>
    {
        Task InviteStudent(Guid groupId, string email);
        Task<bool> AcceptInvite(Guid secret, Guid studentId);
        Task<IEnumerable<GroupInviteItem>> GetInviteList();

        Task<IEnumerable<GroupItem>> GetStudentList();

        Task<IEnumerable<StudentList>> GetStudentList(int year, int semester);

        Task<IEnumerable<Group>> List();
        Task<IEnumerable<StudentInvite>> GetStudents(Guid groupId);

        Task<Result<StudentDescription[]>> ListMembers(int year, int semester, Guid groupId);
        Task<Result> Include(int year, int semester, Guid groupId, Guid userId);
        Task<Result> Exclude(int year, int semester, Guid groupId, Guid userId);
        
        Task<GroupLink[]> Search(string prefix, int? limit);
    }
}