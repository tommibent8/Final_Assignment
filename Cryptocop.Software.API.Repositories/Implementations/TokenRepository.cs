using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Repositories.Interfaces;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class TokenRepository : ITokenRepository
{
    public Task<JwtTokenDto> CreateNewTokenAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsTokenBlacklistedAsync(int tokenId)
    {
        throw new NotImplementedException();
    }

    public Task VoidTokenAsync(int tokenId)
    {
        throw new NotImplementedException();
    }
}