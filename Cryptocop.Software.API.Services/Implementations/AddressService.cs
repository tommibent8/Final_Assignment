using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;

    public AddressService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<IEnumerable<AddressDto>> GetAllAddressesAsync(string email)
    {
        return await _addressRepository.GetAllAddressesAsync(email);
    }

    public async Task AddAddressAsync(string email, AddressInputModel address)
    {
        await _addressRepository.AddAddressAsync(email, address);
    }

    public async Task DeleteAddressAsync(string email, int addressId)
    {
        await _addressRepository.DeleteAddressAsync(email, addressId);
    }
}