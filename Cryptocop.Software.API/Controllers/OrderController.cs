using System.Security.Claims;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocop.Software.API.Controllers;

[Authorize]
[Route("api/orders")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    
    private string GetUserEmail()
    {
        var email = User.FindFirstValue("email")
                    ?? User.FindFirstValue("sub")
                    ?? User.FindFirstValue(ClaimTypes.Email);
        if (email == null) throw new UnauthorizedAccessException("Invalid token.");
        return email;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var email = GetUserEmail();
        var orders = await _orderService.GetOrdersAsync(email);
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderInputModel order)
    {
        var email = GetUserEmail();
        await _orderService.CreateNewOrderAsync(email, order);
        return StatusCode(201, new { message = "Order placed successfully" });
    }
    
}