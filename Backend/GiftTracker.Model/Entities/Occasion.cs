namespace GiftTracker.Model.Entities;

// Table: occasion (e.g. a birthday or Christmas for one recipient)
public class Occasion
{
    public Occasion(int id) { Id = id; }
    public int Id { get; set; }

    // The profile who made the occasion in the app
    public int CreatedById { get; set; }

    // Who gets the gifts: a profile (RecipientId) OR, for someone without an account, just a name
    // (RecipientName). Exactly one of the two is set – the database checks it (chk_occasion_recipient).
    public int? RecipientId { get; set; }
    public string? RecipientName { get; set; }

    public string Name { get; set; } = "";
    public DateOnly EventDate { get; set; }

    // "birthday", "christmas", "anniversary" or "other"
    public string Type { get; set; } = "";
    public string? Notes { get; set; }

    // True when the date has passed. Past occasions are locked: the controllers refuse to change
    // them or their gifts (deleting the whole occasion is still allowed).
    public bool IsPast()
    {
        return EventDate < DateOnly.FromDateTime(DateTime.Today);
    }
}
