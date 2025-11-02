using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class OrderRepository : IOrderRepository
{
    public Task<IEnumerable<OrderDto>> GetOrdersAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<OrderDto> CreateNewOrderAsync(string email, OrderInputModel order)
    {
        throw new NotImplementedException();
    }
}