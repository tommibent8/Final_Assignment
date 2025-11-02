using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class ShoppingCartService : IShoppingCartService
{
    public Task<IEnumerable<ShoppingCartItemDto>> GetCartItemsAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task AddCartItemAsync(string email, ShoppingCartItemInputModel shoppingCartItemItem)
    {
        throw new NotImplementedException();
    }

    public Task RemoveCartItemAsync(string email, int id)
    {
        throw new NotImplementedException();
    }

    public Task UpdateCartItemQuantityAsync(string email, int id, float quantity)
    {
        throw new NotImplementedException();
    }

    public Task ClearCartAsync(string email)
    {
        throw new NotImplementedException();
    }
}