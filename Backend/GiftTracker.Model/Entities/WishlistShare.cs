namespace GiftTracker.Model.Entities;

// Table: wishlist_share (junction table: which groups can see which wishlists)
// No id of its own – the primary key is (WishlistId, GroupId).
public class WishlistShare
{
    public int WishlistId { get; set; }
    public int GroupId { get; set; }
}
