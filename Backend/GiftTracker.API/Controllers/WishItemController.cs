using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WishItemController : ControllerBase
{
    protected WishItemRepository Repository { get; }

    public WishItemController(WishItemRepository repository)
    {
        Repository = repository;
    }

    [HttpGet("{id}")]
    public ActionResult<WishItem> GetWishItem([FromRoute] int id)
    {
        WishItem? w = Repository.GetWishItemById(id);
        if (w == null)
        {
            return NotFound($"Wish item with id {id} not found");
        }
        return Ok(w);
    }

    [HttpGet]
    public ActionResult<IEnumerable<WishItem>> GetWishItems()
    {
        return Ok(Repository.GetWishItems());
    }

    // Returns the new wish item including the id the database gave it
    [HttpPost]
    public ActionResult<WishItem> Post([FromBody] WishItem w)
    {
        if (w == null)
        {
            return BadRequest("Wish item info not correct");
        }
        int newId = Repository.InsertWishItem(w);
        return Ok(Repository.GetWishItemById(newId));
    }

    [HttpPut]
    public ActionResult UpdateWishItem([FromBody] WishItem w)
    {
        if (w == null)
        {
            return BadRequest("Wish item info not correct");
        }
        WishItem? existing = Repository.GetWishItemById(w.Id);
        if (existing == null)
        {
            return NotFound($"Wish item with id {w.Id} not found");
        }
        bool status = Repository.UpdateWishItem(w);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteWishItem([FromRoute] int id)
    {
        WishItem? existing = Repository.GetWishItemById(id);
        if (existing == null)
        {
            return NotFound($"Wish item with id {id} not found");
        }
        bool status = Repository.DeleteWishItem(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete wish item with id {id}");
    }
}
