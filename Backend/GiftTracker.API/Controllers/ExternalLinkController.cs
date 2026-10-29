using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExternalLinkController : ControllerBase
{
    protected ExternalLinkRepository Repository { get; }

    public ExternalLinkController(ExternalLinkRepository repository)
    {
        Repository = repository;
    }

    [HttpGet("{id}")]
    public ActionResult<ExternalLink> GetExternalLink([FromRoute] int id)
    {
        ExternalLink? e = Repository.GetExternalLinkById(id);
        if (e == null)
        {
            return NotFound($"External link with id {id} not found");
        }
        return Ok(e);
    }

    [HttpGet]
    public ActionResult<IEnumerable<ExternalLink>> GetExternalLinks()
    {
        return Ok(Repository.GetExternalLinks());
    }

    // Returns the new external link including the id the database gave it
    [HttpPost]
    public ActionResult<ExternalLink> Post([FromBody] ExternalLink e)
    {
        if (e == null)
        {
            return BadRequest("External link info not correct");
        }
        int newId = Repository.InsertExternalLink(e);
        return Ok(Repository.GetExternalLinkById(newId));
    }

    [HttpPut]
    public ActionResult UpdateExternalLink([FromBody] ExternalLink e)
    {
        if (e == null)
        {
            return BadRequest("External link info not correct");
        }
        ExternalLink? existing = Repository.GetExternalLinkById(e.Id);
        if (existing == null)
        {
            return NotFound($"External link with id {e.Id} not found");
        }
        bool status = Repository.UpdateExternalLink(e);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteExternalLink([FromRoute] int id)
    {
        ExternalLink? existing = Repository.GetExternalLinkById(id);
        if (existing == null)
        {
            return NotFound($"External link with id {id} not found");
        }
        bool status = Repository.DeleteExternalLink(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete external link with id {id}");
    }
}
