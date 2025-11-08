using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Services.Helpers;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class CryptoCurrencyService : ICryptoCurrencyService
{
    private readonly HttpClient _httpClient;
    private static readonly string[] AvailableCryptocurrencies = { "BTC", "ETH", "USDT", "LINK" };

    public CryptoCurrencyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<CryptoCurrencyDto>> GetAvailableCryptocurrenciesAsync()
    {
        // Call Messari API to get all cryptocurrencies
        var response = await _httpClient.GetAsync("https://data.messari.io/api/v1/assets?fields=id,symbol,name,slug,metrics/market_data/price_usd,profile/general/overview/project_details");

        response.EnsureSuccessStatusCode();

        // Deserialize and flatten the response
        var allCryptocurrencies = await response.DeserializeJsonToList<CryptoCurrencyDto>(flatten: true);

        // Filter to only return available cryptocurrencies (BTC, ETH, USDT, LINK)
        var filteredCryptocurrencies = allCryptocurrencies
            .Where(c => AvailableCryptocurrencies.Contains(c.Symbol, StringComparer.OrdinalIgnoreCase))
            .ToList();

        return filteredCryptocurrencies;
    }
}