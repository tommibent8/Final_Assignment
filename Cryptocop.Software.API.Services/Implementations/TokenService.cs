using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cryptocop.Software.API.Services.Implementations;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public Task<string> GenerateJwtTokenAsync(UserDto user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim("fullName", user.FullName),
            new Claim("userId", user.Id.ToString()),
            new Claim("tokenId", user.TokenId.ToString())
        };
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"])),
            signingCredentials: credentials
        );
        
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(jwt);
        //throw new NotImplementedException();
    }
}