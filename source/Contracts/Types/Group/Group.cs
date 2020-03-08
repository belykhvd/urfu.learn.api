using Contracts.Types.Common;

namespace Contracts.Types.Group
{
    public class Group : DbEntity
    {
        public string OfficialName { get; set; }
        public string PopularName { get; set; }
        public int Year { get; set; }
    }
}