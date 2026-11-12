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

    // Sends a friend request. Returns the new connection including the id the database gave it.
    // The database already stops a request to yourself (chk_connection_not_self) and a second
    // connection between the same two people in either direction (uk_connection_pair).
    [HttpPost]
    public ActionResult<Connection> Post([FromBody] Connection c)
    {
        if (c == null)
        {
            return BadRequest("Connection info not correct");
        }
        // Rule: a friendship always starts as a request – nobody can make someone their friend directly
        if (c.Status != "pending")
        {
            return BadRequest("A new friend request must have status 'pending'");
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
        // Rule: a request stays between the same two people – only the status may change
        if (c.RequesterId != existing.RequesterId || c.AddresseeId != existing.AddresseeId)
        {
            return BadRequest("The people in a friend request can't be changed");
        }
        // Rule: the only change allowed is accepting a pending request (pending -> accepted).
        // To decline a request or remove a friend, DELETE the connection instead.
        if (existing.Status == "accepted" && c.Status != "accepted")
        {
            return BadRequest("An accepted friendship can't go back to pending – delete it instead");
        }
        // Note: without login tokens the API can't check that it is the addressee who accepts.
        // The frontend only shows the Accept button to the addressee.
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
