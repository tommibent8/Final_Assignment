using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Middleware;

public class JwtBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public JwtBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IJwtTokenService jwtTokenService)
    {
        // Only check requests that actually carry a JWT
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tokenIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "tokenId");
            if (tokenIdClaim != null && int.TryParse(tokenIdClaim.Value, out var tokenId))
            {
                var isBlacklisted = await jwtTokenService.IsTokenBlacklistedAsync(tokenId);
                if (isBlacklisted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token is blacklisted");
                    return; // Stop pipeline
                }
            }
        }

        // Continue normally
        await _next(context);
    }
}