using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocop.Software.API.Controllers;

[Authorize]
[Route("api/cart")]
[ApiController]
public class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _shoppingCartService;

    public ShoppingCartController(IShoppingCartService shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCartItems()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (email == null) return Unauthorized();

        try
        {
            var cartItems = await _shoppingCartService.GetCartItemsAsync(email);
            return Ok(cartItems);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving cart items", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddCartItem([FromBody] ShoppingCartItemInputModel input)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (email == null) return Unauthorized();

        try
        {
            await _shoppingCartService.AddCartItemAsync(email, input);
            return Ok(new { message = "Item added to cart successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error adding item to cart", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveCartItem(int id)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (email == null) return Unauthorized();

        try
        {
            await _shoppingCartService.RemoveCartItemAsync(email, id);
            return Ok(new { message = "Item removed from cart successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error removing item from cart", error = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateCartItemQuantity(int id, [FromBody] float quantity)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (email == null) return Unauthorized();

        try
        {
            await _shoppingCartService.UpdateCartItemQuantityAsync(email, id, quantity);
            return Ok(new { message = "Cart item quantity updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating cart item quantity", error = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (email == null) return Unauthorized();

        try
        {
            await _shoppingCartService.ClearCartAsync(email);
            return Ok(new { message = "Cart cleared successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error clearing cart", error = ex.Message });
        }
    }
}