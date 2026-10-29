using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProfileController : ControllerBase
{
    protected ProfileRepository Repository { get; }

    public ProfileController(ProfileRepository repository)
    {
        Repository = repository;
    }

    [HttpGet("{id}")]
    public ActionResult<Profile> GetProfile([FromRoute] int id)
    {
        Profile? p = Repository.GetProfileById(id);
        if (p == null)
        {
            return NotFound($"Profile with id {id} not found");
        }
        return Ok(p);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Profile>> GetProfiles()
    {
        return Ok(Repository.GetProfiles());
    }

    // Returns the new profile including the id the database gave it
    [HttpPost]
    public ActionResult<Profile> Post([FromBody] Profile p)
    {
        if (p == null)
        {
            return BadRequest("Profile info not correct");
        }
        if (string.IsNullOrWhiteSpace(p.Password) || p.Password.Length < 8)
        {
            return BadRequest("Password must be at least 8 characters");
        }
        int newId = Repository.InsertProfile(p);
        return Ok(Repository.GetProfileById(newId));
    }

    [HttpPut]
    public ActionResult UpdateProfile([FromBody] Profile p)
    {
        if (p == null)
        {
            return BadRequest("Profile info not correct");
        }
        Profile? existing = Repository.GetProfileById(p.Id);
        if (existing == null)
        {
            return NotFound($"Profile with id {p.Id} not found");
        }
        bool status = Repository.UpdateProfile(p);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteProfile([FromRoute] int id)
    {
        Profile? existing = Repository.GetProfileById(id);
        if (existing == null)
        {
            return NotFound($"Profile with id {id} not found");
        }
        bool status = Repository.DeleteProfile(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete profile with id {id}");
    }
}
