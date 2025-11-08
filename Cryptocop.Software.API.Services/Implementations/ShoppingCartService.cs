using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Helpers;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly HttpClient _httpClient;

    public ShoppingCartService(IShoppingCartRepository shoppingCartRepository, HttpClient httpClient)
    {
        _shoppingCartRepository = shoppingCartRepository;
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<ShoppingCartItemDto>> GetCartItemsAsync(string email)
    {
        return await _shoppingCartRepository.GetCartItemsAsync(email);
    }

    public async Task AddCartItemAsync(string email, ShoppingCartItemInputModel shoppingCartItemItem)
    {
        // Call Messari API to get current price for this cryptocurrency
      /*  var response = await _httpClient.GetAsync($"https://data.messari.io/api/v1/assets/{shoppingCartItemItem.ProductIdentifier}/metrics/market-data");

        response.EnsureSuccessStatusCode();

        // Deserialize to get the price
        var cryptoData = await response.DeserializeJsonToObject<CryptoCurrencyDto>(flatten: true);*/

        // Add to cart with current price
        //await _shoppingCartRepository.AddCartItemAsync(email, shoppingCartItemItem, cryptoData.PriceInUsd);
        await _shoppingCartRepository.AddCartItemAsync(email, shoppingCartItemItem, 102);
    }

    public async Task RemoveCartItemAsync(string email, int id)
    {
        await _shoppingCartRepository.RemoveCartItemAsync(email, id);
    }

    public async Task UpdateCartItemQuantityAsync(string email, int id, float quantity)
    {
        await _shoppingCartRepository.UpdateCartItemQuantityAsync(email, id, quantity);
    }

    public async Task ClearCartAsync(string email)
    {
        await _shoppingCartRepository.ClearCartAsync(email);
    }
}