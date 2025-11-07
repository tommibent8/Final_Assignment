using System.Security.Claims;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocop.Software.API.Controllers;

[Route("api/payments")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }
    
    private string GetUserEmail()
    {
        var email = User.FindFirstValue("email")
                    ?? User.FindFirstValue("sub")
                    ?? User.FindFirstValue(ClaimTypes.Email);
        if (email == null)
            throw new UnauthorizedAccessException("Invalid token.");
        return email;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllPayments()
    {
        var email = GetUserEmail();
        var cards = await _paymentService.GetStoredPaymentCardsAsync(email);
        return Ok(cards);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddPayment([FromBody] PaymentCardInputModel input)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var email = GetUserEmail();
        await _paymentService.AddPaymentCardAsync(email, input);
        return StatusCode(201);
    }
    
}