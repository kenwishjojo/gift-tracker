namespace GiftTracker.Model.Entities;

// Table: group_member
// A member is either a user (ProfileId) or a guest (GuestName) – never both.
public class GroupMember
{
    public GroupMember(int id) { Id = id; }
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int? ProfileId { get; set; }
    public string? GuestName { get; set; }

    // "owner", "member" or "guest"
    public string Role { get; set; } = "";
}
