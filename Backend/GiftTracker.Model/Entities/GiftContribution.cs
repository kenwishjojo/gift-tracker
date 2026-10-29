namespace GiftTracker.Model.Entities;

// Table: gift_contribution (junction table: who paid how much towards a gift)
// No id of its own – the primary key is (GiftId, GroupMemberId).
public class GiftContribution
{
    public int GiftId { get; set; }
    public int GroupMemberId { get; set; }
    public decimal Amount { get; set; }
}
