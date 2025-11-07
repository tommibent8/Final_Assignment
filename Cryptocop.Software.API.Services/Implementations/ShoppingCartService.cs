using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _cartRepository;

    public ShoppingCartService(IShoppingCartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }
    public Task<IEnumerable<ShoppingCartItemDto>> GetCartItemsAsync(string email)
    {
        return _cartRepository.GetCartItemsAsync(email);
    }

    public Task AddCartItemAsync(string email, ShoppingCartItemInputModel shoppingCartItemItem)
    {
        return _cartRepository.AddCartItemAsync(email, shoppingCartItemItem, priceInUsd: 1.0f);
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