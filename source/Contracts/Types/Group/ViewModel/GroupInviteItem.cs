namespace Contracts.Types.Group.ViewModel
{
    public class GroupInviteItem
    {
        public Group Group { get; set; }
        public StudentInvite[] Invites { get; set; }
    }
}