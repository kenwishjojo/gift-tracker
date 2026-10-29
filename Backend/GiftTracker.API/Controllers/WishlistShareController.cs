using GiftTracker.Model.Entities;
using GiftTracker.Model.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.API.Controllers;

// Junction table, so a share is found by two ids: /api/WishlistShare/{wishlistId}/{groupId}
[Route("api/[controller]")]
[ApiController]
public class WishlistShareController : ControllerBase
{
    protected WishlistShareRepository Repository { get; }

    public WishlistShareController(WishlistShareRepository repository)
    {
        Repository = repository;
    }

    [HttpGet("{wishlistId}/{groupId}")]
    public ActionResult<WishlistShare> GetWishlistShare([FromRoute] int wishlistId, [FromRoute] int groupId)
    {
        WishlistShare? share = Repository.GetWishlistShare(wishlistId, groupId);
        if (share == null)
        {
            return NotFound($"Wishlist {wishlistId} is not shared with group {groupId}");
        }
        return Ok(share);
    }

    [HttpGet]
    public ActionResult<IEnumerable<WishlistShare>> GetWishlistShares()
    {
        return Ok(Repository.GetWishlistShares());
    }

    [HttpPost]
    public ActionResult Post([FromBody] WishlistShare share)
    {
        if (share == null)
        {
            return BadRequest("Wishlist share info not correct");
        }
        bool status = Repository.InsertWishlistShare(share);
        if (status)
        {
            return Ok();
        }
        return BadRequest();
    }

    [HttpDelete("{wishlistId}/{groupId}")]
    public ActionResult DeleteWishlistShare([FromRoute] int wishlistId, [FromRoute] int groupId)
    {
        WishlistShare? existing = Repository.GetWishlistShare(wishlistId, groupId);
        if (existing == null)
        {
            return NotFound($"Wishlist {wishlistId} is not shared with group {groupId}");
        }
        bool status = Repository.DeleteWishlistShare(wishlistId, groupId);
        if (status)
        {
            return NoContent();
        }
        return BadRequest("Unable to delete wishlist share");
    }
}
