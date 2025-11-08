using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly IQueueService _queueService;

    public OrderService(IOrderRepository orderRepository, IShoppingCartRepository shoppingCartRepository, IQueueService queueService)
    {
        _orderRepository = orderRepository;
        _shoppingCartRepository = shoppingCartRepository;
        _queueService = queueService;
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersAsync(string email)
    {
        return await _orderRepository.GetOrdersAsync(email);
    }

    public async Task CreateNewOrderAsync(string email, OrderInputModel order)
    {
        // Create the order
        var createdOrder = await _orderRepository.CreateNewOrderAsync(email, order);

        // Clear the shopping cart
        await _shoppingCartRepository.DeleteCartAsync(email);

        // Publish message to RabbitMQ with routing key 'create-order'
        await _queueService.PublishMessageAsync("create-order", createdOrder);
    }
}