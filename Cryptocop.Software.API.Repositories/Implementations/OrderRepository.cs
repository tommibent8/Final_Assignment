using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.Entities;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Contexts;
using Cryptocop.Software.API.Repositories.Helpers;
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
        var orders = await _dbContext.Orders
            .Where(o => o.Email == email)
            .Include(o => o.OrderItems)
            .Select(o => new OrderDto
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
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductIdentifier = oi.ProductIdentifier,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            })
            .ToListAsync();

        return orders;
    }

    public async Task<OrderDto> CreateNewOrderAsync(string email, OrderInputModel orderInput)
    {
        // Retrieve user
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Retrieve address
        var address = await _dbContext.Addresses
            .FirstOrDefaultAsync(a => a.Id == orderInput.AddressId && a.UserId == user.Id);
        if (address == null)
        {
            throw new InvalidOperationException("Address not found.");
        }

        // Retrieve payment card
        var paymentCard = await _dbContext.PaymentCards
            .FirstOrDefaultAsync(pc => pc.Id == orderInput.PaymentCardId && pc.UserId == user.Id);
        if (paymentCard == null)
        {
            throw new InvalidOperationException("Payment card not found.");
        }

        // Get cart items
        var cartItems = await _dbContext.ShoppingCartItems
            .Where(sci => sci.ShoppingCart.UserId == user.Id)
            .ToListAsync();

        if (!cartItems.Any())
        {
            throw new InvalidOperationException("Shopping cart is empty.");
        }

        // Calculate total price
        var totalPrice = cartItems.Sum(ci => ci.Quantity * ci.UnitPrice);

        // Create order with masked credit card
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
            MaskedCreditCard = PaymentCardHelper.MaskPaymentCard(paymentCard.CardNumber),
            OrderDate = DateTime.UtcNow,
            TotalPrice = totalPrice
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        // Create order items
        var orderItems = cartItems.Select(ci => new OrderItem
        {
            OrderId = order.Id,
            ProductIdentifier = ci.ProductIdentifier,
            Quantity = ci.Quantity,
            UnitPrice = ci.UnitPrice,
            TotalPrice = ci.Quantity * ci.UnitPrice
        }).ToList();

        _dbContext.OrderItems.AddRange(orderItems);
        await _dbContext.SaveChangesAsync();

        // Return order DTO with unmasked card number
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
            CreditCard = paymentCard.CardNumber, // Unmasked for return
            OrderDate = order.OrderDate.ToString("dd.MM.yyyy"),
            TotalPrice = order.TotalPrice,
            OrderItems = orderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductIdentifier = oi.ProductIdentifier,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };
    }
}