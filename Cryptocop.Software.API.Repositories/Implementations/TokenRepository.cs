using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.Entities;
using Cryptocop.Software.API.Repositories.Contexts;
using Cryptocop.Software.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class TokenRepository : ITokenRepository
{
    private readonly CryptocopDbContext _dbContext;

    public TokenRepository(CryptocopDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<JwtTokenDto> CreateNewTokenAsync()
    {
        var token = new JwtToken
        {
            Blacklisted = false
        };

        _dbContext.JwtTokens.Add(token);
        await _dbContext.SaveChangesAsync();

        return new JwtTokenDto
        {
            Id = token.Id,
            Blacklisted = token.Blacklisted
        };    
    }

    public async Task<bool> IsTokenBlacklistedAsync(int tokenId)
    {
        var token = await _dbContext.JwtTokens.FirstOrDefaultAsync(t => t.Id == tokenId);
        return token?.Blacklisted ?? false;
        
    }

    public async Task VoidTokenAsync(int tokenId)
    {
        var token = await _dbContext.JwtTokens.FirstOrDefaultAsync(t => t.Id == tokenId);
        if (token != null)
        {
            token.Blacklisted = true;
            await _dbContext.SaveChangesAsync();
        }
    }
}