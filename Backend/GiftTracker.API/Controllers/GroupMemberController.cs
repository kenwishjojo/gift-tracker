using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

// Rule: a gift group has exactly one member with role "owner".
// The owner is added together with the group (POST /api/GiftGroup), so here we make sure
// nobody adds a second owner, and that the owner is never removed or turned into a viewer.
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
        GroupMember? m = Repository.GetGroupMemberById(id);
        if (m == null)
        {
            return NotFound($"Group member with id {id} not found");
        }
        return Ok(m);
    }

    [HttpGet]
    public ActionResult<IEnumerable<GroupMember>> GetGroupMembers()
    {
        return Ok(Repository.GetGroupMembers());
    }

    // Returns the new group member including the id the database gave it
    [HttpPost]
    public ActionResult<GroupMember> Post([FromBody] GroupMember m)
    {
        if (m == null)
        {
            return BadRequest("Group member info not correct");
        }
        if (m.Role == "owner" && Repository.GetOwnerOfGroup(m.GroupId) != null)
        {
            return BadRequest($"Gift group {m.GroupId} already has an owner");
        }
        int newId = Repository.InsertGroupMember(m);
        return Ok(Repository.GetGroupMemberById(newId));
    }

    [HttpPut]
    public ActionResult UpdateGroupMember([FromBody] GroupMember m)
    {
        if (m == null)
        {
            return BadRequest("Group member info not correct");
        }
        GroupMember? existing = Repository.GetGroupMemberById(m.Id);
        if (existing == null)
        {
            return NotFound($"Group member with id {m.Id} not found");
        }
        // The owner must stay the owner of the same group, or that group would have no owner
        if (existing.Role == "owner" && (m.Role != "owner" || m.GroupId != existing.GroupId))
        {
            return BadRequest("The owner can't be changed to a viewer or moved to another group");
        }
        // A viewer can't become owner if the group already has one
        if (m.Role == "owner")
        {
            GroupMember? owner = Repository.GetOwnerOfGroup(m.GroupId);
            if (owner != null && owner.Id != m.Id)
            {
                return BadRequest($"Gift group {m.GroupId} already has an owner");
            }
        }
        bool status = Repository.UpdateGroupMember(m);
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
        if (existing.Role == "owner")
        {
            return BadRequest("The owner can't leave the group – delete the gift group instead");
        }
        bool status = Repository.DeleteGroupMember(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete group member with id {id}");
    }
}
