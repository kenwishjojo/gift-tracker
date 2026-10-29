using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PreferenceItemController : ControllerBase
{
    protected PreferenceItemRepository Repository { get; }

    public PreferenceItemController(PreferenceItemRepository repository)
    {
        Repository = repository;
    }

    [HttpGet("{id}")]
    public ActionResult<PreferenceItem> GetPreferenceItem([FromRoute] int id)
    {
        PreferenceItem? pi = Repository.GetPreferenceItemById(id);
        if (pi == null)
        {
            return NotFound($"Preference item with id {id} not found");
        }
        return Ok(pi);
    }

    [HttpGet]
    public ActionResult<IEnumerable<PreferenceItem>> GetPreferenceItems()
    {
        return Ok(Repository.GetPreferenceItems());
    }

    // Returns the new preference item including the id the database gave it
    [HttpPost]
    public ActionResult<PreferenceItem> Post([FromBody] PreferenceItem pi)
    {
        if (pi == null)
        {
            return BadRequest("Preference item info not correct");
        }
        int newId = Repository.InsertPreferenceItem(pi);
        return Ok(Repository.GetPreferenceItemById(newId));
    }

    [HttpPut]
    public ActionResult UpdatePreferenceItem([FromBody] PreferenceItem pi)
    {
        if (pi == null)
        {
            return BadRequest("Preference item info not correct");
        }
        PreferenceItem? existing = Repository.GetPreferenceItemById(pi.Id);
        if (existing == null)
        {
            return NotFound($"Preference item with id {pi.Id} not found");
        }
        bool status = Repository.UpdatePreferenceItem(pi);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    [HttpDelete("{id}")]
    public ActionResult DeletePreferenceItem([FromRoute] int id)
    {
        PreferenceItem? existing = Repository.GetPreferenceItemById(id);
        if (existing == null)
        {
            return NotFound($"Preference item with id {id} not found");
        }
        bool status = Repository.DeletePreferenceItem(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete preference item with id {id}");
    }
}
