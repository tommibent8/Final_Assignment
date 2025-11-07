using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocop.Software.API.Controllers;

[Route("api/account")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }
    
    // Register new User
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _accountService.CreateUserAsync(input);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    
    // Sign in user
    [AllowAnonymous]
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] LoginInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _accountService.AuthenticateUserAsync(input);
            return Ok(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
    
    // Sign out user (blacklist token)
    [Authorize]
    [HttpGet("signout")]
    public async Task<IActionResult> SignOut()
    {
        var tokenIdClaim = User.Claims.FirstOrDefault(c => c.Type == "tokenId");
        if (tokenIdClaim == null || !int.TryParse(tokenIdClaim.Value, out var tokenId))
        {
            return BadRequest("Invalid token.");
        }

        await _accountService.LogoutAsync(tokenId);
        return Ok(new { message = "User successfully logged out." });
    }
}