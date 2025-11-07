using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    public Task<IEnumerable<OrderDto>> GetOrdersAsync(string email)
    {
        return _orderRepository.GetOrdersAsync(email);
    }

    public Task CreateNewOrderAsync(string email, OrderInputModel order)
    {
        return _orderRepository.CreateNewOrderAsync(email, order);
    }
}