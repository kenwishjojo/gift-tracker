namespace GiftTracker.Model.Entities;

// Table: occasion_invite (a friend invited to an occasion by its creator).
// You can see an occasion when you created it or are invited to it – never when it is for you.
public class OccasionInvite
{
    public OccasionInvite(int id) { Id = id; }
    public int Id { get; set; }
    public int OccasionId { get; set; }

    // The invited profile
    public int ProfileId { get; set; }

    // Set by the database (default now())
    public DateTime InvitedAt { get; set; }
}
