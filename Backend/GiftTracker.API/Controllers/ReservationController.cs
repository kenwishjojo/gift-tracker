using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservationController : ControllerBase
{
    protected ReservationRepository Repository { get; }

    public ReservationController(ReservationRepository repository)
    {
        Repository = repository;
    }

    [HttpGet("{id}")]
    public ActionResult<Reservation> GetReservation([FromRoute] int id)
    {
        Reservation? r = Repository.GetReservationById(id);
        if (r == null)
        {
            return NotFound($"Reservation with id {id} not found");
        }
        return Ok(r);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Reservation>> GetReservations()
    {
        return Ok(Repository.GetReservations());
    }

    // Returns the new reservation including the id the database gave it
    [HttpPost]
    public ActionResult<Reservation> Post([FromBody] Reservation r)
    {
        if (r == null)
        {
            return BadRequest("Reservation info not correct");
        }
        int newId = Repository.InsertReservation(r);
        return Ok(Repository.GetReservationById(newId));
    }

    [HttpPut]
    public ActionResult UpdateReservation([FromBody] Reservation r)
    {
        if (r == null)
        {
            return BadRequest("Reservation info not correct");
        }
        Reservation? existing = Repository.GetReservationById(r.Id);
        if (existing == null)
        {
            return NotFound($"Reservation with id {r.Id} not found");
        }
        bool status = Repository.UpdateReservation(r);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteReservation([FromRoute] int id)
    {
        Reservation? existing = Repository.GetReservationById(id);
        if (existing == null)
        {
            return NotFound($"Reservation with id {id} not found");
        }
        bool status = Repository.DeleteReservation(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete reservation with id {id}");
    }
}
