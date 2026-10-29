namespace GiftTracker.Model.Entities;

// Table: gift_group (a family or friend group)
public class GiftGroup
{
    public GiftGroup(int id) { Id = id; }
    public int Id { get; set; }
    public string Name { get; set; } = "";

    // Leave empty when creating a group and the API makes one up
    public string? InviteCode { get; set; }

    // Set by the database (default now())
    public DateTime CreatedAt { get; set; }
}
