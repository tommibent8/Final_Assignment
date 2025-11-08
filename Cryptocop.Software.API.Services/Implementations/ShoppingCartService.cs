using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly ICryptoCurrencyService _cryptoService;

    public ShoppingCartService(IShoppingCartRepository cartRepository, ICryptoCurrencyService cryptoService)
    {
        _cartRepository = cartRepository;
        _cryptoService = cryptoService;
    }
    public Task<IEnumerable<ShoppingCartItemDto>> GetCartItemsAsync(string email)
    {
        return _cartRepository.GetCartItemsAsync(email);
    }

    public async Task AddCartItemAsync(string email, ShoppingCartItemInputModel shoppingCartItemItem)
    {
        // Get live crypto data
        var allCryptos = await _cryptoService.GetAvailableCryptocurrenciesAsync();

        // Find the price for the crypto symbol being added
        var selectedCrypto = allCryptos
            .FirstOrDefault(c => c.Symbol.Equals(shoppingCartItemItem.ProductIdentifier, StringComparison.OrdinalIgnoreCase));

        if (selectedCrypto == null)
            throw new InvalidOperationException("Invalid cryptocurrency symbol.");

        // Use the live price from the mock API
        float priceInUsd = selectedCrypto.PriceInUsd;

        await _cartRepository.AddCartItemAsync(email, shoppingCartItemItem, priceInUsd);
        
    }

    public Task RemoveCartItemAsync(string email, int id)
    {
        return _cartRepository.RemoveCartItemAsync(email, id);
        
    }

    public Task UpdateCartItemQuantityAsync(string email, int id, float quantity)
    {
       return _cartRepository.UpdateCartItemQuantityAsync(email, id, quantity);
    }

    public Task ClearCartAsync(string email)
    {
    return _cartRepository.ClearCartAsync(email);

    }
}