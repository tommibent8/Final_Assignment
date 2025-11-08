using System.Security.Cryptography;
using System.Text;
using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.Entities;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Contexts;
using Cryptocop.Software.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly CryptocopDbContext _dbContext;
    private readonly ITokenRepository _tokenRepository;

    public UserRepository(CryptocopDbContext dbContext, ITokenRepository tokenRepository)
    {
        _dbContext = dbContext;
        _tokenRepository = tokenRepository;
    }
    public async Task<UserDto> CreateUserAsync(RegisterInputModel inputModel)
    {
        // Check if user already exists
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == inputModel.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User already exists.");
        }    
        
        // Hash password
        var hashedPassword = HashPassword(inputModel.Password);

        // Create user entity
        var user = new User
        {
            FullName = inputModel.FullName,
            Email = inputModel.Email,
            HashedPassword = hashedPassword
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Create token entry in DB
        var token = await _tokenRepository.CreateNewTokenAsync();

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            TokenId = token.Id // use token.Id, since token is a JwtTokenDto
        };
        
    }

    public async Task<UserDto> AuthenticateUserAsync(LoginInputModel loginInputModel)
    {
        var hashedPassword = HashPassword(loginInputModel.Password);

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == loginInputModel.Email && u.HashedPassword == hashedPassword);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Create new token entry
        var token = await _tokenRepository.CreateNewTokenAsync();
        
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            TokenId = token.Id 
        };
        
    }
    
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    
    
}