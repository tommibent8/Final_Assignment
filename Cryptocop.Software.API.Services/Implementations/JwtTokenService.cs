using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class JwtTokenService : IJwtTokenService
{
    
    private readonly ITokenRepository _tokenRepository;

    public JwtTokenService(ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }
    
    public async Task<bool> IsTokenBlacklistedAsync(int tokenId)
    {
        return await _tokenRepository.IsTokenBlacklistedAsync(tokenId);
        
        //throw new NotImplementedException();
    }
}