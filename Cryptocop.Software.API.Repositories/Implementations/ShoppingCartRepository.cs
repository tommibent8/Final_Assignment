using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.Entities;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Contexts;
using Cryptocop.Software.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly CryptocopDbContext _dbContext;

    public ShoppingCartRepository(CryptocopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ShoppingCartItemDto>> GetCartItemsAsync(string email)
    {
        var cartItems = await _dbContext.ShoppingCartItems
            .Where(sci => sci.ShoppingCart.User.Email == email)
            .Select(sci => new ShoppingCartItemDto
            {
                Id = sci.Id,
                ProductIdentifier = sci.ProductIdentifier,
                Quantity = sci.Quantity,
                UnitPrice = sci.UnitPrice,
                TotalPrice = sci.Quantity * sci.UnitPrice
            })
            .ToListAsync();

        return cartItems;
    }

    public async Task AddCartItemAsync(string email, ShoppingCartItemInputModel shoppingCartItemItem, float priceInUsd)
    {
        var user = await _dbContext.Users
            .Include(u => u.ShoppingCarts)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Get or create shopping cart for user
        var cart = user.ShoppingCarts.FirstOrDefault();
        if (cart == null)
        {
            cart = new ShoppingCart { UserId = user.Id };
            _dbContext.ShoppingCarts.Add(cart);
            await _dbContext.SaveChangesAsync();
        }

        var newItem = new ShoppingCartItem
        {
            ShoppingCartId = cart.Id,
            ProductIdentifier = shoppingCartItemItem.ProductIdentifier,
            Quantity = shoppingCartItemItem.Quantity ?? 0,
            UnitPrice = priceInUsd
        };

        _dbContext.ShoppingCartItems.Add(newItem);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveCartItemAsync(string email, int id)
    {
        var cartItem = await _dbContext.ShoppingCartItems
            .FirstOrDefaultAsync(sci => sci.Id == id && sci.ShoppingCart.User.Email == email);

        if (cartItem != null)
        {
            _dbContext.ShoppingCartItems.Remove(cartItem);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateCartItemQuantityAsync(string email, int id, float quantity)
    {
        var cartItem = await _dbContext.ShoppingCartItems
            .FirstOrDefaultAsync(sci => sci.Id == id && sci.ShoppingCart.User.Email == email);

        if (cartItem != null)
        {
            cartItem.Quantity = quantity;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(string email)
    {
        var cartItems = await _dbContext.ShoppingCartItems
            .Where(sci => sci.ShoppingCart.User.Email == email)
            .ToListAsync();

        _dbContext.ShoppingCartItems.RemoveRange(cartItems);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteCartAsync(string email)
    {
        // Same as clear cart - removes all items from the shopping cart
        await ClearCartAsync(email);
    }
}