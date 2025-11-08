using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.Entities;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Contexts;
using Cryptocop.Software.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class AddressRepository : IAddressRepository
{
    private readonly CryptocopDbContext _dbContext;

    public AddressRepository(CryptocopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AddressDto>> GetAllAddressesAsync(string email)
    {
        var addresses = await _dbContext.Addresses
            .Where(a => a.User.Email == email)
            .Select(a => new AddressDto
            {
                Id = a.Id,
                StreetName = a.StreetName,
                HouseNumber = a.HouseNumber,
                ZipCode = a.ZipCode,
                Country = a.Country,
                City = a.City
            })
            .ToListAsync();

        return addresses;
    }

    public async Task AddAddressAsync(string email, AddressInputModel address)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var newAddress = new Address
        {
            UserId = user.Id,
            StreetName = address.StreetName,
            HouseNumber = address.HouseNumber,
            ZipCode = address.ZipCode,
            Country = address.Country,
            City = address.City
        };

        _dbContext.Addresses.Add(newAddress);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAddressAsync(string email, int addressId)
    {
        var address = await _dbContext.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.User.Email == email);

        if (address != null)
        {
            _dbContext.Addresses.Remove(address);
            await _dbContext.SaveChangesAsync();
        }
    }
}