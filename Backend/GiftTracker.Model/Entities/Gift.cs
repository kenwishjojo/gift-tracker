namespace GiftTracker.Model.Entities;

// Table: gift
// The recipient is not stored here – it is the RecipientId of the gift's occasion.
public class Gift
{
    public Gift(int id) { Id = id; }
    public int Id { get; set; }
    public int OccasionId { get; set; }

    // The profile who buys the gift (can't be the occasion's recipient – checked in GiftController)
    public int BuyerId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }

    // Empty if the price isn't known yet
    public decimal? Price { get; set; }
    public string? Link { get; set; }

    // "idea", "planned", "bought" or "given"
    public string Status { get; set; } = "";
}
