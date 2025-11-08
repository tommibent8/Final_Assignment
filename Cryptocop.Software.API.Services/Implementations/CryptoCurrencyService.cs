using System.Text.Json;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Services.Helpers;
using Cryptocop.Software.API.Services.Interfaces;
using Newtonsoft.Json.Linq;

namespace Cryptocop.Software.API.Services.Implementations;

public class CryptoCurrencyService : ICryptoCurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly string[] _allowed = { "BTC", "ETH", "USDT", "LINK" };

    public CryptoCurrencyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://messari-mock-907563a36b7b.herokuapp.com/api/v2/");
    }

    public async Task<IEnumerable<CryptoCurrencyDto>> GetAvailableCryptocurrenciesAsync()
    {
        var response = await _httpClient.GetAsync("assets");

        if (!response.IsSuccessStatusCode)
            return Enumerable.Empty<CryptoCurrencyDto>();

        var json = await response.Content.ReadAsStringAsync();
        var root = JObject.Parse(json);

        var data = root["data"] as JArray;
        if (data == null) return Enumerable.Empty<CryptoCurrencyDto>();

        var cryptos = new List<CryptoCurrencyDto>();

        foreach (var item in data)
        {
            var symbol = item["symbol"]?.ToString()?.ToUpper();
            if (symbol == null || !_allowed.Contains(symbol)) continue;

            var dto = new CryptoCurrencyDto
            {
                Id = item["id"]?.ToString() ?? symbol,
                Symbol = symbol,
                Name = item["name"]?.ToString() ?? symbol,
                Slug = item["slug"]?.ToString() ?? symbol.ToLower(),
                PriceInUsd = (float?)item["metrics"]?["market_data"]?["price_usd"] ?? 0,
                ProjectDetails = item["profile"]?["general"]?["overview"]?["tagline"]?.ToString() ?? "No details available"
            };

            cryptos.Add(dto);
        }

        return cryptos;
    }
}