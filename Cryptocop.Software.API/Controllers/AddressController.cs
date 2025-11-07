using System.Security.Claims;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocop.Software.API.Controllers;

[Route("api/addresses")]
[ApiController]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    private string GetUserEmail()
    {
        var email = User.FindFirstValue("email")
                    ?? User.FindFirstValue("sub")     
                    ?? User.FindFirstValue(ClaimTypes.Email);
        if (email == null) throw new UnauthorizedAccessException("Invalid token.");
        return email;
    }



    [HttpGet]
    public async Task<IActionResult> GetAllAddresses()
    {
        var email = GetUserEmail();
        var addresses = await _addressService.GetAllAddressesAsync(email);
        return Ok(addresses);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddAddress([FromBody] AddressInputModel address)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var email = GetUserEmail();
        await _addressService.AddAddressAsync(email, address);
        return StatusCode(201);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var email = GetUserEmail();
        await _addressService.DeleteAddressAsync(email, id);
        return Ok(new { message = "Address deleted successfully." });
    }

    
}