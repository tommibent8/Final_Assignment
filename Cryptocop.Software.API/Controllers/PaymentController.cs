using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocop.Software.API.Controllers;

[Authorize]
[Route("api/payments")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStoredPaymentCards()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (email == null) return Unauthorized();

        try
        {
            var paymentCards = await _paymentService.GetStoredPaymentCardsAsync(email);
            return Ok(paymentCards);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving payment cards", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddPaymentCard([FromBody] PaymentCardInputModel input)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (email == null) return Unauthorized();

        try
        {
            await _paymentService.AddPaymentCardAsync(email, input);
            return Ok(new { message = "Payment card added successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error adding payment card", error = ex.Message });
        }
    }
}