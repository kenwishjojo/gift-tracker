namespace GiftTracker.Model.Entities;

// Table: reservation ("I'll buy this one" – a wish item can only be reserved once)
public class Reservation
{
    public Reservation(int id) { Id = id; }
    public int Id { get; set; }
    public int WishItemId { get; set; }
    public int GroupMemberId { get; set; }

    // Set by the database (default now())
    public DateTime ReservedAt { get; set; }
}
