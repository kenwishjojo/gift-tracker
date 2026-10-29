namespace GiftTracker.Model.Entities;

// Table: preference_type (e.g. "Clothing size", "Allergy")
public class PreferenceType
{
    public PreferenceType(int id) { Id = id; }
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
