using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class JwtTokenService : IJwtTokenService
{
    public Task<bool> IsTokenBlacklistedAsync(int tokenId)
    {
        throw new NotImplementedException();
    }
}