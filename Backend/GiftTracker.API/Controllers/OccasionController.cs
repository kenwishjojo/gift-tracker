using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OccasionController : ControllerBase
{
    protected OccasionRepository Repository { get; }

    // Needed to check the gifts of an occasion when its recipient is changed
    protected GiftRepository GiftRepository { get; }

    // Needed for the rule "the recipient must be one of the creator's friends"
    protected ConnectionRepository ConnectionRepository { get; }

    public OccasionController(OccasionRepository repository, GiftRepository giftRepository,
        ConnectionRepository connectionRepository)
    {
        Repository = repository;
        GiftRepository = giftRepository;
        ConnectionRepository = connectionRepository;
    }

    [HttpGet("{id}")]
    public ActionResult<Occasion> GetOccasion([FromRoute] int id)
    {
        Occasion? o = Repository.GetOccasionById(id);
        if (o == null)
        {
            return NotFound($"Occasion with id {id} not found");
        }
        return Ok(o);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Occasion>> GetOccasions()
    {
        return Ok(Repository.GetOccasions());
    }

    // Returns the new occasion including the id the database gave it
    [HttpPost]
    public ActionResult<Occasion> Post([FromBody] Occasion o)
    {
        if (o == null)
        {
            return BadRequest("Occasion info not correct");
        }
        string? error = CheckRecipientIsFriend(o);
        if (error != null)
        {
            return BadRequest(error);
        }
        int newId = Repository.InsertOccasion(o);
        return Ok(Repository.GetOccasionById(newId));
    }

    [HttpPut]
    public ActionResult UpdateOccasion([FromBody] Occasion o)
    {
        if (o == null)
        {
            return BadRequest("Occasion info not correct");
        }
        Occasion? existing = Repository.GetOccasionById(o.Id);
        if (existing == null)
        {
            return NotFound($"Occasion with id {o.Id} not found");
        }
        // Rule: a past occasion is locked – it can be viewed and deleted, but not changed
        if (existing.IsPast())
        {
            return BadRequest("This occasion has passed, so it can't be changed any more");
        }
        string? error = CheckRecipientIsFriend(o);
        if (error != null)
        {
            return BadRequest(error);
        }
        // Rule: a gift's buyer can't be the occasion's recipient.
        // If the recipient changes, nobody who is already buying a gift may become the recipient.
        foreach (Gift g in GiftRepository.GetGiftsByOccasionId(o.Id))
        {
            if (o.RecipientId != null && g.BuyerId == o.RecipientId)
            {
                return BadRequest($"Profile {o.RecipientId} is buying gift {g.Id} for this occasion, so they can't be its recipient");
            }
        }
        bool status = Repository.UpdateOccasion(o);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    // Deleting is allowed even for past occasions (to tidy up). Also deletes its gifts and invites.
    [HttpDelete("{id}")]
    public ActionResult DeleteOccasion([FromRoute] int id)
    {
        Occasion? existing = Repository.GetOccasionById(id);
        if (existing == null)
        {
            return NotFound($"Occasion with id {id} not found");
        }
        bool status = Repository.DeleteOccasion(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete occasion with id {id}");
    }

    // Rule: when the occasion is for a profile, that profile must be one of the creator's friends.
    // (An occasion for someone without an account only has a RecipientName – nothing to check.)
    // Returns an error message, or null if the occasion is fine.
    private string? CheckRecipientIsFriend(Occasion o)
    {
        if (o.RecipientId != null && !ConnectionRepository.AreFriends(o.CreatedById, o.RecipientId.Value))
        {
            return "You can only make an occasion for one of your friends (or for someone without an account, by name)";
        }
        return null;
    }
}
