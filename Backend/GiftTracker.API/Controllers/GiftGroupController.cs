using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GiftGroupController : ControllerBase
{
    protected GiftGroupRepository Repository { get; }

    public GiftGroupController(GiftGroupRepository repository)
    {
        Repository = repository;
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
        bool status = Repository.DeleteGiftGroup(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete gift group with id {id}");
    }
}
