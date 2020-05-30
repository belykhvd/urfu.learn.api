using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Common;
using Contracts.Types.Group;

namespace Contracts.Services
{
    public interface IGroupService : IRepo<Group>
    {
        Task<IEnumerable<StudentList>> GetStudentList(int year, int semester);

        Task<IEnumerable<Group>> List();

        Task<Result<StudentDescription[]>> ListMembers(int year, int semester, Guid groupId);
        Task<Result> Include(int year, int semester, Guid groupId, Guid userId);
        Task<Result> Exclude(int year, int semester, Guid groupId, Guid userId);
        
        Task<GroupLink[]> Search(string prefix, int? limit);
    }
}