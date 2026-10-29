using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GiftController : ControllerBase
{
    protected GiftRepository Repository { get; }

    public GiftController(GiftRepository repository)
    {
        Repository = repository;
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
}
