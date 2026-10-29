using System.Text.Json.Serialization;

namespace GiftTracker.Model.Entities;

// Table: profile (a user account)
public class Profile
{
    public Profile(int id) { Id = id; }
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";

    // Only used when signing up (POST). It is hashed before it is saved and is never
    // sent back by the API, so the password hash never leaves the database.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Password { get; set; }

    // Optional – filled in later from the profile page
    public DateOnly? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? GiftNotes { get; set; }

    // Set by the database (default now())
    public DateTime CreatedAt { get; set; }
}
