using System.Security.Claims;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocop.Software.API.Controllers;

[Route("api/cart")]
[ApiController]
[Authorize]
public class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _cartService;

    public ShoppingCartController(IShoppingCartService cartService)
    {
        _cartService = cartService;
    }

    private string GetUserEmail()
    {
        var email = User.FindFirstValue("email")
                    ?? User.FindFirstValue("sub")
                    ?? User.FindFirstValue(ClaimTypes.Email);
        if (email == null) throw new UnauthorizedAccessException("Invalid token.");
        return email;
    }
    
    // GET /api/cart
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        Console.WriteLine("ShoppingCartController reached successfully!");
        var email = GetUserEmail();
        var items = await _cartService.GetCartItemsAsync(email);
        return Ok(items);
    }

 
    // POST /api/cart
    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] ShoppingCartItemInputModel input)
    {
        var email = GetUserEmail();
        await _cartService.AddCartItemAsync(email, input);
        return StatusCode(201);
    }

    // DELETE /api/cart/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveItem(int id)
    {
        var email = GetUserEmail();
        await _cartService.RemoveCartItemAsync(email, id);
        return Ok(new { message = "Item removed" });
    }

    // PATCH /api/cart/{id}
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateQuantity(int id, [FromBody] float quantity)
    {
        var email = GetUserEmail();
        await _cartService.UpdateCartItemQuantityAsync(email, id, quantity);
        return Ok(new { message = "Item quantity updated" });
    }

    // DELETE /api/cart
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var email = GetUserEmail();
        await _cartService.ClearCartAsync(email);
        return Ok(new { message = "Cart cleared" });
    }
}