using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.Entities;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ITokenRepository _tokenRepository;


    public AccountService(IUserRepository userRepository, ITokenService tokenService, ITokenRepository tokenRepository)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _tokenRepository = tokenRepository;
    }

    public async Task<UserDto> CreateUserAsync(RegisterInputModel inputModel)
    {
        // Create user in DB
        var user = await _userRepository.CreateUserAsync(inputModel);

        // Generate JWT token and attach to DTO
        user.Token = await _tokenService.GenerateJwtTokenAsync(user);
        return user;
    }

    public async Task<UserDto> AuthenticateUserAsync(LoginInputModel loginInputModel)
        {
            // Authenticate via repository
            var user = await _userRepository.AuthenticateUserAsync(loginInputModel);

            // Generate JWT for this user
            user.Token = await _tokenService.GenerateJwtTokenAsync(user);
            return user;
        }

        public async Task LogoutAsync(int tokenId)
    {
        // Blacklist (void) the current JWT
        await _tokenRepository.VoidTokenAsync(tokenId);    
    }
}