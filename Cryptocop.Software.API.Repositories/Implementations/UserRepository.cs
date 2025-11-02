using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    public Task<UserDto> CreateUserAsync(RegisterInputModel inputModel)
    {
        throw new NotImplementedException();
    }

    public Task<UserDto> AuthenticateUserAsync(LoginInputModel loginInputModel)
    {
        throw new NotImplementedException();
    }
}