using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GiftController : ControllerBase
{
    protected GiftRepository Repository { get; }

    // Needed to find the gift's recipient (it is stored on the occasion, not on the gift)
    protected OccasionRepository OccasionRepository { get; }

    public GiftController(GiftRepository repository, OccasionRepository occasionRepository)
    {
        Repository = repository;
        OccasionRepository = occasionRepository;
    }

    [HttpGet("{id}")]
    public ActionResult<Gift> GetGift([FromRoute] int id)
    {
        Gift? g = Repository.GetGiftById(id);
        if (g == null)
        {
            return NotFound($"Gift with id {id} not found");
        }
        return Ok(g);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Gift>> GetGifts()
    {
        return Ok(Repository.GetGifts());
    }

    // Returns the new gift including the id the database gave it
    [HttpPost]
    public ActionResult<Gift> Post([FromBody] Gift g)
    {
        if (g == null)
        {
            return BadRequest("Gift info not correct");
        }
        string? error = CheckBuyerIsNotRecipient(g);
        if (error != null)
        {
            return BadRequest(error);
        }
        int newId = Repository.InsertGift(g);
        return Ok(Repository.GetGiftById(newId));
    }

    [HttpPut]
    public ActionResult UpdateGift([FromBody] Gift g)
    {
        if (g == null)
        {
            return BadRequest("Gift info not correct");
        }
        Gift? existing = Repository.GetGiftById(g.Id);
        if (existing == null)
        {
            return NotFound($"Gift with id {g.Id} not found");
        }
        string? error = CheckBuyerIsNotRecipient(g);
        if (error != null)
        {
            return BadRequest(error);
        }
        bool status = Repository.UpdateGift(g);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteGift([FromRoute] int id)
    {
        Gift? existing = Repository.GetGiftById(id);
        if (existing == null)
        {
            return NotFound($"Gift with id {id} not found");
        }
        bool status = Repository.DeleteGift(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete gift with id {id}");
    }

    // Rule: the buyer of a gift can't be the gift's recipient (the occasion's recipient).
    // Returns an error message, or null if the gift is fine.
    private string? CheckBuyerIsNotRecipient(Gift g)
    {
        Occasion? occasion = OccasionRepository.GetOccasionById(g.OccasionId);
        if (occasion == null)
        {
            return $"Occasion with id {g.OccasionId} not found";
        }
        if (g.BuyerId == occasion.RecipientId)
        {
            return "The buyer can't be the recipient of the occasion";
        }
        return null;
    }
}
