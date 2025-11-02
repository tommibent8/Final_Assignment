using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class OrderService : IOrderService
{
    public Task<IEnumerable<OrderDto>> GetOrdersAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task CreateNewOrderAsync(string email, OrderInputModel order)
    {
        throw new NotImplementedException();
    }
}