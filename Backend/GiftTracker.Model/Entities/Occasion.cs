namespace GiftTracker.Model.Entities;

// Table: occasion (a birthday, Christmas, ...)
public class Occasion
{
    public Occasion(int id) { Id = id; }
    public int Id { get; set; }
    public int GroupId { get; set; }

    // The person being celebrated – empty for shared occasions like Christmas
    public int? HonoreeId { get; set; }
    public string Name { get; set; } = "";
    public DateOnly EventDate { get; set; }
    public decimal Budget { get; set; }
}
