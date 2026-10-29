namespace GiftTracker.Model.Entities;

// Table: preference_item (e.g. Clothing size / Shoes / 38)
public class PreferenceItem
{
    public PreferenceItem(int id) { Id = id; }
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public int PreferenceTypeId { get; set; }
    public string? Subject { get; set; }
    public string Value { get; set; } = "";
}
