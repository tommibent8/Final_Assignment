using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class AccountService : IAccountService
{
    public Task<UserDto> CreateUserAsync(RegisterInputModel inputModel)
    {
        throw new NotImplementedException();
    }

    public Task<UserDto> AuthenticateUserAsync(LoginInputModel loginInputModel)
    {
        throw new NotImplementedException();
    }

    public Task LogoutAsync(int tokenId)
    {
        throw new NotImplementedException();
    }
}