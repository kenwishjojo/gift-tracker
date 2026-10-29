namespace GiftTracker.Model.Entities;

// Table: external_link (a wishlist on another site, e.g. Pinterest or Amazon)
public class ExternalLink
{
    public ExternalLink(int id) { Id = id; }
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public string Service { get; set; } = "";
    public string Url { get; set; } = "";
}
