namespace GiftTracker.Model.Entities;

// Table: wishlist
public class Wishlist
{
    public Wishlist(int id) { Id = id; }
    public int Id { get; set; }
    public int OwnerId { get; set; }

    // Empty for an "always welcome" list that isn't tied to one occasion
    public int? OccasionId { get; set; }
    public string Title { get; set; } = "";
}
