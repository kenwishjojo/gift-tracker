using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConnectionController : ControllerBase
{
    protected ConnectionRepository Repository { get; }

    public ConnectionController(ConnectionRepository repository)
    {
        Repository = repository;
    }

    [HttpGet("{id}")]
    public ActionResult<Connection> GetConnection([FromRoute] int id)
    {
        Connection? c = Repository.GetConnectionById(id);
        if (c == null)
        {
            return NotFound($"Connection with id {id} not found");
        }
        return Ok(c);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Connection>> GetConnections()
    {
        return Ok(Repository.GetConnections());
    }

    // Returns the new connection including the id the database gave it
    [HttpPost]
    public ActionResult<Connection> Post([FromBody] Connection c)
    {
        if (c == null)
        {
            return BadRequest("Connection info not correct");
        }
        int newId = Repository.InsertConnection(c);
        return Ok(Repository.GetConnectionById(newId));
    }

    [HttpPut]
    public ActionResult UpdateConnection([FromBody] Connection c)
    {
        if (c == null)
        {
            return BadRequest("Connection info not correct");
        }
        Connection? existing = Repository.GetConnectionById(c.Id);
        if (existing == null)
        {
            return NotFound($"Connection with id {c.Id} not found");
        }
        bool status = Repository.UpdateConnection(c);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteConnection([FromRoute] int id)
    {
        Connection? existing = Repository.GetConnectionById(id);
        if (existing == null)
        {
            return NotFound($"Connection with id {id} not found");
        }
        bool status = Repository.DeleteConnection(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete connection with id {id}");
    }
}
