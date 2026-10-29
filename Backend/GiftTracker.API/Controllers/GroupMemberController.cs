using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GroupMemberController : ControllerBase
{
    protected GroupMemberRepository Repository { get; }

    public GroupMemberController(GroupMemberRepository repository)
    {
        Repository = repository;
    }

    [HttpGet("{id}")]
    public ActionResult<GroupMember> GetGroupMember([FromRoute] int id)
    {
        GroupMember? g = Repository.GetGroupMemberById(id);
        if (g == null)
        {
            return NotFound($"Group member with id {id} not found");
        }
        return Ok(g);
    }

    [HttpGet]
    public ActionResult<IEnumerable<GroupMember>> GetGroupMembers()
    {
        return Ok(Repository.GetGroupMembers());
    }

    // Returns the new group member including the id the database gave it
    [HttpPost]
    public ActionResult<GroupMember> Post([FromBody] GroupMember g)
    {
        if (g == null)
        {
            return BadRequest("Group member info not correct");
        }
        int newId = Repository.InsertGroupMember(g);
        return Ok(Repository.GetGroupMemberById(newId));
    }

    [HttpPut]
    public ActionResult UpdateGroupMember([FromBody] GroupMember g)
    {
        if (g == null)
        {
            return BadRequest("Group member info not correct");
        }
        GroupMember? existing = Repository.GetGroupMemberById(g.Id);
        if (existing == null)
        {
            return NotFound($"Group member with id {g.Id} not found");
        }
        bool status = Repository.UpdateGroupMember(g);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteGroupMember([FromRoute] int id)
    {
        GroupMember? existing = Repository.GetGroupMemberById(id);
        if (existing == null)
        {
            return NotFound($"Group member with id {id} not found");
        }
        bool status = Repository.DeleteGroupMember(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete group member with id {id}");
    }
}
