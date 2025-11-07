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
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new InvalidOperationException("User not found.");

        var cart = await _dbContext.ShoppingCarts
            .Include(c => c.ShoppingCartItems)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (cart == null || !cart.ShoppingCartItems.Any())
            return Enumerable.Empty<ShoppingCartItemDto>();

        return cart.ShoppingCartItems.Select(i => new ShoppingCartItemDto
        {
            Id = i.Id,
            ProductIdentifier = i.ProductIdentifier,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TotalPrice = i.TotalPrice // now from DB
        });
    }

    public async Task AddCartItemAsync(string email, ShoppingCartItemInputModel item, float priceInUsd)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new InvalidOperationException("User not found.");

        var cart = await _dbContext.ShoppingCarts
            .Include(c => c.ShoppingCartItems)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (cart == null)
        {
            cart = new ShoppingCart { UserId = user.Id, ShoppingCartItems = new List<ShoppingCartItem>() };
            _dbContext.ShoppingCarts.Add(cart);
        }

        var existingItem = cart.ShoppingCartItems
            .FirstOrDefault(i => i.ProductIdentifier == item.ProductIdentifier);

        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity ?? 1;
            existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice; // update total
        }
        else
        {
            var newItem = new ShoppingCartItem
            {
                ProductIdentifier = item.ProductIdentifier,
                Quantity = item.Quantity ?? 1,
                UnitPrice = priceInUsd,
                TotalPrice = (item.Quantity ?? 1) * priceInUsd // store total price
            };
            cart.ShoppingCartItems.Add(newItem);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveCartItemAsync(string email, int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new InvalidOperationException("User not found.");

        var item = await _dbContext.ShoppingCartItems
            .Include(i => i.ShoppingCart)
            .FirstOrDefaultAsync(i => i.Id == id && i.ShoppingCart.UserId == user.Id);

        if (item != null)
        {
            _dbContext.ShoppingCartItems.Remove(item);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateCartItemQuantityAsync(string email, int id, float quantity)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new InvalidOperationException("User not found.");

        var item = await _dbContext.ShoppingCartItems
            .Include(i => i.ShoppingCart)
            .FirstOrDefaultAsync(i => i.Id == id && i.ShoppingCart.UserId == user.Id);

        if (item == null) return;

        item.Quantity = quantity;
        item.TotalPrice = item.Quantity * item.UnitPrice; // keep total in sync
        await _dbContext.SaveChangesAsync();
    }

    public async Task ClearCartAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new InvalidOperationException("User not found.");

        var cartItems = await _dbContext.ShoppingCartItems
            .Where(i => i.ShoppingCart.UserId == user.Id)
            .ToListAsync();

        _dbContext.ShoppingCartItems.RemoveRange(cartItems);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteCartAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new InvalidOperationException("User not found.");

        var cart = await _dbContext.ShoppingCarts
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (cart != null)
        {
            _dbContext.ShoppingCarts.Remove(cart);
            await _dbContext.SaveChangesAsync();
        }
        
    }
}
