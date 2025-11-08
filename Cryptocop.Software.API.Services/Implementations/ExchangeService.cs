using Cryptocop.Software.API.Models;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Services.Helpers;
using Cryptocop.Software.API.Services.Interfaces;
using Newtonsoft.Json.Linq;

namespace Cryptocop.Software.API.Services.Implementations;

public class ExchangeService : IExchangeService
{
    private readonly HttpClient _httpClient;

    public ExchangeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://messari-mock-907563a36b7b.herokuapp.com/api/v1/");
    }

    public async Task<Envelope<ExchangeDto>> GetExchangesAsync(int pageNumber = 1)
    {
        var response = await _httpClient.GetAsync("markets");

        if (!response.IsSuccessStatusCode)
        {
            return new Envelope<ExchangeDto>
            {
                PageNumber = pageNumber,
                Items = Enumerable.Empty<ExchangeDto>()
            };
        }

        var json = await response.Content.ReadAsStringAsync();
        var root = JObject.Parse(json);

        var data = root["data"] as JArray;
        var status = root["status"]?["timestamp"]?.ToString();

        DateTime? lastTrade = null;
        if (DateTime.TryParse(status, out var parsedDate))
        {
            lastTrade = parsedDate;
        }

        if (data == null)
        {
            return new Envelope<ExchangeDto>
            {
                PageNumber = pageNumber,
                Items = Enumerable.Empty<ExchangeDto>()
            };
        }

        var exchanges = data.Select(item => new ExchangeDto
        {
            Id = item["id"]?.ToString(),
            Name = item["name"]?.ToString(),
            Slug = item["slug"]?.ToString(),
            PriceInUsd = (float?)item["price_usd"] ?? 0,
            LastTrade = lastTrade
        }).ToList();

        return new Envelope<ExchangeDto>
        {
            PageNumber = pageNumber,
            Items = exchanges
        };
    }
}