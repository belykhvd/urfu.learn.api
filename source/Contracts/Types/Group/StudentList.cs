using Contracts.Types.Common;

namespace Contracts.Types.Group
{
    public class StudentList
    {
        public Link Group { get; set; }
        public Link[] Users { get; set; }
    }
}