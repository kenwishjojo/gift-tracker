namespace GiftTracker.Model.Entities;

// Table: gift
public class Gift
{
    public Gift(int id) { Id = id; }
    public int Id { get; set; }
    public int OccasionId { get; set; }
    public int RecipientId { get; set; }

    // Empty if the gift isn't from a wishlist
    public int? WishItemId { get; set; }
    public string Description { get; set; } = "";

    // "individual" or "shared"
    public string PurchaseType { get; set; } = "";

    // "idea", "bought" or "given"
    public string Status { get; set; } = "";
}
