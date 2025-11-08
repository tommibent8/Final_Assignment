using Cryptocop.Software.API.Models;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Services.Helpers;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class ExchangeService : IExchangeService
{
    private readonly HttpClient _httpClient;

    public ExchangeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Envelope<ExchangeDto>> GetExchangesAsync(int pageNumber = 1)
    {
        // Call Messari API with pagination
        var response = await _httpClient.GetAsync($"https://data.messari.io/api/v1/markets?page={pageNumber}&fields=exchange_id,exchange_name,exchange_slug,base_asset_symbol,price_usd,last_trade_at");

        response.EnsureSuccessStatusCode();

        // Deserialize and flatten the response
        var exchanges = await response.DeserializeJsonToList<ExchangeDto>(flatten: true);

        // Create and return envelope
        var envelope = new Envelope<ExchangeDto>
        {
            PageNumber = pageNumber,
            Items = exchanges
        };

        return envelope;
    }
}