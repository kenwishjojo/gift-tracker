using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PreferenceTypeController : ControllerBase
{
    protected PreferenceTypeRepository Repository { get; }

    public PreferenceTypeController(PreferenceTypeRepository repository)
    {
        Repository = repository;
    }

    [HttpGet("{id}")]
    public ActionResult<PreferenceType> GetPreferenceType([FromRoute] int id)
    {
        PreferenceType? pt = Repository.GetPreferenceTypeById(id);
        if (pt == null)
        {
            return NotFound($"Preference type with id {id} not found");
        }
        return Ok(pt);
    }

    [HttpGet]
    public ActionResult<IEnumerable<PreferenceType>> GetPreferenceTypes()
    {
        return Ok(Repository.GetPreferenceTypes());
    }

    // Returns the new preference type including the id the database gave it
    [HttpPost]
    public ActionResult<PreferenceType> Post([FromBody] PreferenceType pt)
    {
        if (pt == null)
        {
            return BadRequest("Preference type info not correct");
        }
        int newId = Repository.InsertPreferenceType(pt);
        return Ok(Repository.GetPreferenceTypeById(newId));
    }

    [HttpPut]
    public ActionResult UpdatePreferenceType([FromBody] PreferenceType pt)
    {
        if (pt == null)
        {
            return BadRequest("Preference type info not correct");
        }
        PreferenceType? existing = Repository.GetPreferenceTypeById(pt.Id);
        if (existing == null)
        {
            return NotFound($"Preference type with id {pt.Id} not found");
        }
        bool status = Repository.UpdatePreferenceType(pt);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    [HttpDelete("{id}")]
    public ActionResult DeletePreferenceType([FromRoute] int id)
    {
        PreferenceType? existing = Repository.GetPreferenceTypeById(id);
        if (existing == null)
        {
            return NotFound($"Preference type with id {id} not found");
        }
        bool status = Repository.DeletePreferenceType(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete preference type with id {id}");
    }
}
