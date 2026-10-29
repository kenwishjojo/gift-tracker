using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

// Junction table, so a contribution is found by two ids: /api/GiftContribution/{giftId}/{groupMemberId}
[Route("api/[controller]")]
[ApiController]
public class GiftContributionController : ControllerBase
{
    protected GiftContributionRepository Repository { get; }

    public GiftContributionController(GiftContributionRepository repository)
    {
        Repository = repository;
    }

    [HttpGet("{giftId}/{groupMemberId}")]
    public ActionResult<GiftContribution> GetGiftContribution([FromRoute] int giftId, [FromRoute] int groupMemberId)
    {
        GiftContribution? contribution = Repository.GetGiftContribution(giftId, groupMemberId);
        if (contribution == null)
        {
            return NotFound($"No contribution from group member {groupMemberId} to gift {giftId}");
        }
        return Ok(contribution);
    }

    [HttpGet]
    public ActionResult<IEnumerable<GiftContribution>> GetGiftContributions()
    {
        return Ok(Repository.GetGiftContributions());
    }

    [HttpPost]
    public ActionResult Post([FromBody] GiftContribution contribution)
    {
        if (contribution == null)
        {
            return BadRequest("Gift contribution info not correct");
        }
        bool status = Repository.InsertGiftContribution(contribution);
        if (status)
        {
            return Ok();
        }
        return BadRequest();
    }

    // Changes the amount of an existing contribution
    [HttpPut]
    public ActionResult UpdateGiftContribution([FromBody] GiftContribution contribution)
    {
        if (contribution == null)
        {
            return BadRequest("Gift contribution info not correct");
        }
        GiftContribution? existing = Repository.GetGiftContribution(contribution.GiftId, contribution.GroupMemberId);
        if (existing == null)
        {
            return NotFound($"No contribution from group member {contribution.GroupMemberId} to gift {contribution.GiftId}");
        }
        bool status = Repository.UpdateGiftContribution(contribution);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    [HttpDelete("{giftId}/{groupMemberId}")]
    public ActionResult DeleteGiftContribution([FromRoute] int giftId, [FromRoute] int groupMemberId)
    {
        GiftContribution? existing = Repository.GetGiftContribution(giftId, groupMemberId);
        if (existing == null)
        {
            return NotFound($"No contribution from group member {groupMemberId} to gift {giftId}");
        }
        bool status = Repository.DeleteGiftContribution(giftId, groupMemberId);
        if (status)
        {
            return NoContent();
        }
        return BadRequest("Unable to delete gift contribution");
    }
}
