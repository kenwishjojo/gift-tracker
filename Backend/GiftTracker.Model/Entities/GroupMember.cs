namespace GiftTracker.Model.Entities;

// Table: group_member (a profile that is part of a gift group)
public class GroupMember
{
    public GroupMember(int id) { Id = id; }
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int ProfileId { get; set; }

    // "owner" or "viewer" – every group has exactly one owner (checked in GroupMemberController)
    public string Role { get; set; } = "";

    // Set by the database (default now())
    public DateTime JoinedAt { get; set; }
}
