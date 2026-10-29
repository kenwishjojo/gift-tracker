namespace GiftTracker.Model.Entities;

// Table: wish_item (one thing on a wishlist)
public class WishItem
{
    public WishItem(int id) { Id = id; }
    public int Id { get; set; }
    public int WishlistId { get; set; }
    public string Name { get; set; } = "";
    public string Link { get; set; } = "";
    public decimal Price { get; set; }

    // 1 = wanted most
    public int Priority { get; set; }
}
