using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocop.Software.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cryptocurrencies")]
public class CryptoCurrencyController : ControllerBase
{
    private readonly ICryptoCurrencyService _cryptoCurrencyService;

    public CryptoCurrencyController(ICryptoCurrencyService cryptoCurrencyService)
    {
        _cryptoCurrencyService = cryptoCurrencyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableCryptocurrencies()
    {
        try
        {
            var cryptocurrencies = await _cryptoCurrencyService.GetAvailableCryptocurrenciesAsync();
            return Ok(cryptocurrencies);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving cryptocurrencies", error = ex.Message });
        }
    }
}