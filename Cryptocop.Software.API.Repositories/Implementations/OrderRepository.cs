using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.Entities;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Contexts;
using Cryptocop.Software.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class OrderRepository : IOrderRepository
{
    private readonly CryptocopDbContext _dbContext;

    public OrderRepository(CryptocopDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IEnumerable<OrderDto>> GetOrdersAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new InvalidOperationException("User not found.");

        var orders = await _dbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Email == user.Email)
            .ToListAsync();

        return orders.Select(o => new OrderDto
        {
            Id = o.Id,
            Email = o.Email,
            FullName = o.FullName,
            StreetName = o.StreetName,
            HouseNumber = o.HouseNumber,
            ZipCode = o.ZipCode,
            Country = o.Country,
            City = o.City,
            CardholderName = o.CardHolderName,
            CreditCard = o.MaskedCreditCard,
            OrderDate = o.OrderDate.ToString("dd.MM.yyyy"),
            TotalPrice = o.TotalPrice,
            OrderItems = o.OrderItems.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductIdentifier = i.ProductIdentifier,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        });    }

    public async Task<OrderDto> CreateNewOrderAsync(string email, OrderInputModel input)
    {
        var user = await _dbContext.Users
            .Include(u => u.Addresses)
            .Include(u => u.PaymentCards)
            .FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new InvalidOperationException("User not found.");

        var address = await _dbContext.Addresses.FirstOrDefaultAsync(a => a.Id == input.AddressId && a.UserId == user.Id);
        var paymentCard = await _dbContext.PaymentCards.FirstOrDefaultAsync(c => c.Id == input.PaymentCardId && c.UserId == user.Id);
        if (address == null || paymentCard == null) throw new InvalidOperationException("Invalid address or payment card.");

        var cart = await _dbContext.ShoppingCarts
            .Include(c => c.ShoppingCartItems)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (cart == null || !cart.ShoppingCartItems.Any())
            throw new InvalidOperationException("Shopping cart is empty.");

        var totalPrice = cart.ShoppingCartItems.Sum(i => i.TotalPrice);

        // Mask credit card
        var maskedCard = paymentCard.CardNumber.Length > 4
            ? $"**** **** **** {paymentCard.CardNumber[^4..]}"
            : paymentCard.CardNumber;

        // Create order
        var order = new Order
        {
            Email = user.Email,
            FullName = user.FullName,
            StreetName = address.StreetName,
            HouseNumber = address.HouseNumber,
            ZipCode = address.ZipCode,
            Country = address.Country,
            City = address.City,
            CardHolderName = paymentCard.CardholderName,
            MaskedCreditCard = maskedCard,
            OrderDate = DateTime.UtcNow,
            TotalPrice = totalPrice,
            OrderItems = cart.ShoppingCartItems.Select(i => new OrderItem
            {
                ProductIdentifier = i.ProductIdentifier,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };

        _dbContext.Orders.Add(order);
        _dbContext.ShoppingCartItems.RemoveRange(cart.ShoppingCartItems); // clear cart
        await _dbContext.SaveChangesAsync();

        return new OrderDto
        {
            Id = order.Id,
            Email = order.Email,
            FullName = order.FullName,
            StreetName = order.StreetName,
            HouseNumber = order.HouseNumber,
            ZipCode = order.ZipCode,
            Country = order.Country,
            City = order.City,
            CardholderName = order.CardHolderName,
            CreditCard = order.MaskedCreditCard,
            OrderDate = order.OrderDate.ToString("dd.MM.yyyy"),
            TotalPrice = order.TotalPrice,
            OrderItems = order.OrderItems.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductIdentifier = i.ProductIdentifier,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };    }
}