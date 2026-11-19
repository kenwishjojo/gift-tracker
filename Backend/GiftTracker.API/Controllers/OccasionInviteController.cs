using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

// Who is invited to which occasion.
// GET all, GET {id}, POST and DELETE {id} like the other tables. There is no plain PUT, because an
// invite has nothing to change – instead PUT occasion/{occasionId} replaces an occasion's whole list.
[Route("api/[controller]")]
[ApiController]
public class OccasionInviteController : ControllerBase
{
    protected OccasionInviteRepository Repository { get; }

    // Needed for the rules below (the occasion's creator, recipient and date, and who is friends)
    protected OccasionRepository OccasionRepository { get; }
    protected ConnectionRepository ConnectionRepository { get; }

    public OccasionInviteController(OccasionInviteRepository repository, OccasionRepository occasionRepository,
        ConnectionRepository connectionRepository)
    {
        Repository = repository;
        OccasionRepository = occasionRepository;
        ConnectionRepository = connectionRepository;
    }

    [HttpGet("{id}")]
    public ActionResult<OccasionInvite> GetInvite([FromRoute] int id)
    {
        OccasionInvite? i = Repository.GetInviteById(id);
        if (i == null)
        {
            return NotFound($"Invite with id {id} not found");
        }
        return Ok(i);
    }

    [HttpGet]
    public ActionResult<IEnumerable<OccasionInvite>> GetInvites()
    {
        return Ok(Repository.GetInvites());
    }

    // Invites one person. Returns the new invite including the id the database gave it.
    [HttpPost]
    public ActionResult<OccasionInvite> Post([FromBody] OccasionInvite i)
    {
        if (i == null)
        {
            return BadRequest("Invite info not correct");
        }
        Occasion? occasion = OccasionRepository.GetOccasionById(i.OccasionId);
        if (occasion == null)
        {
            return NotFound($"Occasion with id {i.OccasionId} not found");
        }
        string? error = CheckInvite(occasion, i.ProfileId);
        if (error != null)
        {
            return BadRequest(error);
        }
        int newId = Repository.InsertInvite(i);
        return Ok(Repository.GetInviteById(newId));
    }

    // Replaces the whole invite list of one occasion, e.g. PUT /api/OccasionInvite/occasion/3
    // with the body [3, 6, 7]. Every profile is checked first; then the list is saved in one transaction.
    [HttpPut("occasion/{occasionId}")]
    public ActionResult ReplaceInvites([FromRoute] int occasionId, [FromBody] List<int> profileIds)
    {
        Occasion? occasion = OccasionRepository.GetOccasionById(occasionId);
        if (occasion == null)
        {
            return NotFound($"Occasion with id {occasionId} not found");
        }
        foreach (int profileId in profileIds)
        {
            string? error = CheckInvite(occasion, profileId);
            if (error != null)
            {
                return BadRequest(error);
            }
        }
        Repository.ReplaceInvites(occasionId, profileIds.Distinct().ToList());
        return Ok();
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteInvite([FromRoute] int id)
    {
        OccasionInvite? existing = Repository.GetInviteById(id);
        if (existing == null)
        {
            return NotFound($"Invite with id {id} not found");
        }
        Occasion? occasion = OccasionRepository.GetOccasionById(existing.OccasionId);
        if (occasion != null && occasion.IsPast())
        {
            return BadRequest("This occasion has passed, so it can't be changed any more");
        }
        bool status = Repository.DeleteInvite(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete invite with id {id}");
    }

    // The rules for inviting one person. Returns an error message, or null if the invite is fine.
    private string? CheckInvite(Occasion occasion, int profileId)
    {
        // Rule: a past occasion is locked
        if (occasion.IsPast())
        {
            return "This occasion has passed, so it can't be changed any more";
        }
        // Rule: the creator sees their occasion anyway, so they aren't invited
        if (profileId == occasion.CreatedById)
        {
            return "The creator of an occasion can't be invited to it";
        }
        // Rule: the person the occasion is for must never see it (the gifts are a surprise)
        if (occasion.RecipientId != null && profileId == occasion.RecipientId)
        {
            return "The person the occasion is for can't be invited – the gifts are a surprise";
        }
        // Rule: you can only invite your friends
        if (!ConnectionRepository.AreFriends(occasion.CreatedById, profileId))
        {
            return $"Profile {profileId} isn't a friend of the occasion's creator, so they can't be invited";
        }
        return null;
    }
}
