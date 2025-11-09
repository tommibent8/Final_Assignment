using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IQueueService _queueService;


    public OrderService(IOrderRepository orderRepository, IQueueService  queueService)
    {
        _orderRepository = orderRepository;
        _queueService = queueService;
    }
    public Task<IEnumerable<OrderDto>> GetOrdersAsync(string email)
    {
        return _orderRepository.GetOrdersAsync(email);
    }

    public async Task CreateNewOrderAsync(string email, OrderInputModel order)
    {
        var newOrder = await _orderRepository.CreateNewOrderAsync(email, order);

        var message = new
        {
            OrderId = newOrder.Id,
            Email = newOrder.Email,
            FullName = newOrder.FullName,
            Address = $"{newOrder.StreetName}, {newOrder.HouseNumber}",
            City = newOrder.City,
            ZipCode = newOrder.ZipCode,
            Country = newOrder.Country,
            TotalPrice = newOrder.TotalPrice,
            OrderDate = newOrder.OrderDate,
            Items = newOrder.OrderItems.Select(i => new
            {
                i.ProductIdentifier,
                i.Quantity,
                i.UnitPrice
            })
        };

        await _queueService.PublishMessageAsync("create-order", message);
    }
}