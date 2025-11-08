using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocop.Software.API.Controllers;

[ApiController]
[Authorize]
[Route("api/cryptocurrencies")]
public class CryptoCurrencyController : ControllerBase
{
    private readonly ICryptoCurrencyService _cryptoService;

    public CryptoCurrencyController(ICryptoCurrencyService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cryptos = await _cryptoService.GetAvailableCryptocurrenciesAsync();
        return Ok(cryptos);
    }
    
}