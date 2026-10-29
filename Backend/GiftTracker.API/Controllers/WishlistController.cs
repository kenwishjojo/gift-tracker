using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WishlistController : ControllerBase
{
    protected WishlistRepository Repository { get; }

    public WishlistController(WishlistRepository repository)
    {
        Repository = repository;
    }

    [HttpGet("{id}")]
    public ActionResult<Wishlist> GetWishlist([FromRoute] int id)
    {
        Wishlist? w = Repository.GetWishlistById(id);
        if (w == null)
        {
            return NotFound($"Wishlist with id {id} not found");
        }
        return Ok(w);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Wishlist>> GetWishlists()
    {
        return Ok(Repository.GetWishlists());
    }

    // Returns the new wishlist including the id the database gave it
    [HttpPost]
    public ActionResult<Wishlist> Post([FromBody] Wishlist w)
    {
        if (w == null)
        {
            return BadRequest("Wishlist info not correct");
        }
        int newId = Repository.InsertWishlist(w);
        return Ok(Repository.GetWishlistById(newId));
    }

    [HttpPut]
    public ActionResult UpdateWishlist([FromBody] Wishlist w)
    {
        if (w == null)
        {
            return BadRequest("Wishlist info not correct");
        }
        Wishlist? existing = Repository.GetWishlistById(w.Id);
        if (existing == null)
        {
            return NotFound($"Wishlist with id {w.Id} not found");
        }
        bool status = Repository.UpdateWishlist(w);
        if (status)
        {
            return Ok();
        }
        return BadRequest("Something went wrong");
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteWishlist([FromRoute] int id)
    {
        Wishlist? existing = Repository.GetWishlistById(id);
        if (existing == null)
        {
            return NotFound($"Wishlist with id {id} not found");
        }
        bool status = Repository.DeleteWishlist(id);
        if (status)
        {
            return NoContent();
        }
        return BadRequest($"Unable to delete wishlist with id {id}");
    }
}
