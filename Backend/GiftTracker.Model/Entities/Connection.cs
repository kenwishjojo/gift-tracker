namespace GiftTracker.Model.Entities;

// Table: connection (a friend request from one profile to another)
public class Connection
{
    public Connection(int id) { Id = id; }
    public int Id { get; set; }

    // The profile who sent the request
    public int RequesterId { get; set; }

    // The profile who received the request
    public int AddresseeId { get; set; }

    // "pending" or "accepted"
    public string Status { get; set; } = "";

    // Set by the database (default now())
    public DateTime CreatedAt { get; set; }
}
