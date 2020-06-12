using Contracts.Types.Common;

namespace Contracts.Types.Group.ViewModel
{
    public class GroupInviteItem : Group
    {
        public StudentInvite[] InviteList { get; set; }
        public Link[] CourseList { get; set; }
    }
}