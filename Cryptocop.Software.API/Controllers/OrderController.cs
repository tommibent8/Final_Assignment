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

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (email == null) return Unauthorized();

        try
        {
            var orders = await _orderService.GetOrdersAsync(email);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving orders", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderInputModel input)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (email == null) return Unauthorized();

        try
        {
            await _orderService.CreateNewOrderAsync(email, input);
            return Ok(new { message = "Order created successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating order", error = ex.Message });
        }
    }
}