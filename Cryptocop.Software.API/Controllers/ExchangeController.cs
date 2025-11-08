using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocop.Software.API.Controllers;

[Authorize]
[Route("api/exchanges")]
[ApiController]
public class ExchangeController : ControllerBase
{
    private readonly IExchangeService _exchangeService;

    public ExchangeController(IExchangeService exchangeService)
    {
        _exchangeService = exchangeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetExchanges([FromQuery] int pageNumber = 1)
    {
        try
        {
            var exchanges = await _exchangeService.GetExchangesAsync(pageNumber);
            return Ok(exchanges);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving exchanges", error = ex.Message });
        }
    }
}