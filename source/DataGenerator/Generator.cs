using System.Threading.Tasks;
using Contracts.Services;

namespace DataGenerator
{
    public class Generator
    {
        private static readonly ICourseService courseService;

        public async Task Generate()
        {
            courseService.Save()
        }
    }
}