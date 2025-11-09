using System.Text;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Cryptocop.Software.API.Middleware;

public static class CryptoCopAuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>(); 
                        
                        logger.LogInformation("On validated check");
                        
                        var jwtTokenService = context.HttpContext
                            .RequestServices.GetRequiredService<IJwtTokenService>();
                        
                        var tokenIdClaim = context.Principal?.FindFirst("tokenId");
                        
                        if (tokenIdClaim != null && int.TryParse(tokenIdClaim.Value, out var tokenId))
                        {
                            logger.LogInformation("Found token id  {id} check if blacklisted",tokenId);
                            
                            var isBlacklisted = await jwtTokenService.IsTokenBlacklistedAsync(tokenId);
                            if (isBlacklisted)
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                await context.Response.WriteAsync("Token is blacklisted");
                                
                            }
                        }
                    }
                };
            });
        
        return services;
    }
    
    
}