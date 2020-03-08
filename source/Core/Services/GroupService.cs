using System;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Common;
using Contracts.Types.Group;
using Core.Repo;
using Microsoft.Extensions.Configuration;

namespace Core.Services
{
    public class GroupService : CrudRepo<Group>, IGroupService
    {
        protected GroupService(IConfiguration config) : base(config, @"""group""")
        {
        }

        public Task<OperationStatus> Include(Guid groupId, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<OperationStatus> Exclude(Guid groupId, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<StudentDescription[]> GetMembers(Guid groupId)
        {
            throw new NotImplementedException();
        }

        public Task<GroupDescription[]> Search(string prefix, int? limit)
        {
            throw new NotImplementedException();
        }
    }
}