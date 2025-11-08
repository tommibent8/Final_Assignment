using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocop.Software.API.Controllers;

[Route("api/exchanges")]
[ApiController]
[Authorize]
public class ExchangeController : ControllerBase
{
    private readonly IExchangeService _exchangeService;

    public ExchangeController(IExchangeService exchangeService)
    {
        _exchangeService = exchangeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1)
    {
        var result = await _exchangeService.GetExchangesAsync(pageNumber);
        return Ok(result);
    }

}