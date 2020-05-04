using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Repo;
using Contracts.Types.Common;
using Contracts.Types.Course;

namespace Contracts.Services
{
    public interface ICourseService : IRepo<Course>
    {
        Task<IEnumerable<Link>> Select();
    }
}