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
            return NotFound($"Group with id {id} not found");
        }
        return Ok(g);
    }

    [HttpGet]
    public ActionResult<IEnumerable<GiftGroup>> GetGiftGroups()
    {
        return Ok(Repository.GetGiftGroups());
    }

    // Returns the new group including the id the database gave it
    [HttpPost]
    public ActionResult<GiftGroup> Post([FromBody] GiftGroup g)
    {
        if (g == null)
        {
            return BadRequest("Group info not correct");
        }
        // Make up an invite code if none was given, e.g. "GRP-4F7A2C"
        if (string.IsNullOrWhiteSpace(g.InviteCode))
        {
            g.InviteCode = "GRP-" + Guid.NewGuid().ToString("N")[..6].ToUpper();
        }
        int newId = Repository.InsertGiftGroup(g);
        return Ok(Repository.GetGiftGroupById(newId));
    }

    [HttpPut]
    public ActionResult UpdateGiftGroup([FromBody] GiftGroup g)
    {
        if (g == null)
        {
            return BadRequest("Group info not correct");
        }
        GiftGroup? existing = Repository.GetGiftGroupById(g.Id);
        if (existing == null)
        {
            return NotFound($"Group with id {g.Id} not found");
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
            return NotFound($"Group with id {id} not found");
        }
        bool status = Repository.DeleteGiftGroup(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete group with id {id}");
    }
}
