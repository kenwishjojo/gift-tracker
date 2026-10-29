namespace GiftTracker.Model.Entities;

// Table: gift_group (people splitting one gift – "group" is a reserved word in PostgreSQL)
// A gift is a group gift when a gift_group points to it.
public class GiftGroup
{
    public GiftGroup(int id) { Id = id; }
    public int Id { get; set; }

    // The gift the group is splitting (at most one group per gift)
    public int GiftId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }

    // Not a column in gift_group: it is the profile of the member with role "owner".
    // Send it when creating a group (POST) – the API then adds that person as the owner.
    // PUT ignores it (change members with /api/GroupMember instead).
    public int? OwnerId { get; set; }

    // Set by the database (default now())
    public DateTime CreatedAt { get; set; }
}
