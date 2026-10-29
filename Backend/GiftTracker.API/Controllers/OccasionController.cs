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

    public OccasionController(OccasionRepository repository, GiftRepository giftRepository)
    {
        Repository = repository;
        GiftRepository = giftRepository;
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
        // Rule: a gift's buyer can't be the occasion's recipient.
        // If the recipient changes, nobody who is already buying a gift may become the recipient.
        foreach (Gift g in GiftRepository.GetGiftsByOccasionId(o.Id))
        {
            if (g.BuyerId == o.RecipientId)
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
}
