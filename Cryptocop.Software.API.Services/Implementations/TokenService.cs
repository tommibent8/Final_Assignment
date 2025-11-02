using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class TokenService : ITokenService
{
    public Task<string> GenerateJwtTokenAsync(UserDto user)
    {
        throw new NotImplementedException();
    }
}