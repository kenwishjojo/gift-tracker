using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GiftGroupController : ControllerBase
{
    protected GiftGroupRepository Repository { get; }

    // Needed to check a new group against its gift and the gift's occasion
    protected GiftRepository GiftRepository { get; }
    protected OccasionRepository OccasionRepository { get; }

    public GiftGroupController(GiftGroupRepository repository, GiftRepository giftRepository,
        OccasionRepository occasionRepository)
    {
        Repository = repository;
        GiftRepository = giftRepository;
        OccasionRepository = occasionRepository;
    }

    [HttpGet("{id}")]
    public ActionResult<GiftGroup> GetGiftGroup([FromRoute] int id)
    {
        GiftGroup? g = Repository.GetGiftGroupById(id);
        if (g == null)
        {
            return NotFound($"Gift group with id {id} not found");
        }
        return Ok(g);
    }

    [HttpGet]
    public ActionResult<IEnumerable<GiftGroup>> GetGiftGroups()
    {
        return Ok(Repository.GetGiftGroups());
    }

    // Creates the group and makes OwnerId its owner. Returns the new group including its id.
    [HttpPost]
    public ActionResult<GiftGroup> Post([FromBody] GiftGroup g)
    {
        if (g == null)
        {
            return BadRequest("Gift group info not correct");
        }
        // Rule: a gift group has exactly one owner – so we need to know who it is
        if (g.OwnerId == null)
        {
            return BadRequest("ownerId is required: every gift group needs an owner");
        }
        Gift? gift = GiftRepository.GetGiftById(g.GiftId);
        if (gift == null)
        {
            return NotFound($"Gift with id {g.GiftId} not found");
        }
        // Rule: the person who buys the gift is the one who owns the group
        if (g.OwnerId != gift.BuyerId)
        {
            return BadRequest("The owner of a gift group must be the gift's buyer");
        }
        // Rule: the owner is already in the group, so they can't be in memberIds too
        if (g.MemberIds.Contains(g.OwnerId.Value))
        {
            return BadRequest("The owner can't also be in memberIds");
        }
        // Rule: the person the gift is for can't help pay for it (same idea as "buyer can't be the recipient")
        Occasion? occasion = OccasionRepository.GetOccasionById(gift.OccasionId);
        // Rule: a past occasion is locked, including who splits its gifts
        if (occasion != null && occasion.IsPast())
        {
            return BadRequest("This occasion has passed, so its gifts can't be changed any more");
        }
        // (RecipientId is null when the occasion is for someone without an account – nothing to check)
        if (occasion != null && occasion.RecipientId != null && g.MemberIds.Contains(occasion.RecipientId.Value))
        {
            return BadRequest("The person the gift is for can't split it");
        }
        int newId = Repository.InsertGiftGroup(g);
        return Ok(Repository.GetGiftGroupById(newId));
    }

    [HttpPut]
    public ActionResult UpdateGiftGroup([FromBody] GiftGroup g)
    {
        if (g == null)
        {
            return BadRequest("Gift group info not correct");
        }
        GiftGroup? existing = Repository.GetGiftGroupById(g.Id);
        if (existing == null)
        {
            return NotFound($"Gift group with id {g.Id} not found");
        }
        bool status = Repository.UpdateGiftGroup(g);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteGiftGroup([FromRoute] int id)
    {
        GiftGroup? existing = Repository.GetGiftGroupById(id);
        if (existing == null)
        {
            return NotFound($"Gift group with id {id} not found");
        }
        // Rule: a past occasion is locked, including who splits its gifts
        Gift? gift = GiftRepository.GetGiftById(existing.GiftId);
        Occasion? occasion = gift == null ? null : OccasionRepository.GetOccasionById(gift.OccasionId);
        if (occasion != null && occasion.IsPast())
        {
            return BadRequest("This occasion has passed, so its gifts can't be changed any more");
        }
        bool status = Repository.DeleteGiftGroup(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete gift group with id {id}");
    }
}
