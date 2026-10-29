namespace GiftTracker.Model.Entities;

// Table: occasion (e.g. a birthday or Christmas for one recipient)
public class Occasion
{
    public Occasion(int id) { Id = id; }
    public int Id { get; set; }

    // The profile who made the occasion in the app
    public int CreatedById { get; set; }

    // The profile who gets the gifts
    public int RecipientId { get; set; }
    public string Name { get; set; } = "";
    public DateOnly EventDate { get; set; }

    // "birthday", "christmas", "anniversary" or "other"
    public string Type { get; set; } = "";
    public string? Notes { get; set; }
}
