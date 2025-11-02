using Cryptocop.Software.API.Models;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class ExchangeService : IExchangeService
{
    public Task<Envelope<ExchangeDto>> GetExchangesAsync(int pageNumber = 1)
    {
        throw new NotImplementedException();
    }
}