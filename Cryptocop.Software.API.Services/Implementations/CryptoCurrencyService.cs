using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class CryptoCurrencyService : ICryptoCurrencyService
{
    public Task<IEnumerable<CryptoCurrencyDto>> GetAvailableCryptocurrenciesAsync()
    {
        throw new NotImplementedException();
    }
}