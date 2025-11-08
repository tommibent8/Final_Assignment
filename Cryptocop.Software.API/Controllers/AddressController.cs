using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cryptocop.Software.API.Controllers;

[Authorize]
[Route("api/addresses")]
[ApiController]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAddresses()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (email == null) return Unauthorized();

        try
        {
            var addresses = await _addressService.GetAllAddressesAsync(email);
            return Ok(addresses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving addresses", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddAddress([FromBody] AddressInputModel input)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (email == null) return Unauthorized();

        try
        {
            await _addressService.AddAddressAsync(email, input);
            return Ok(new { message = "Address added successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error adding address", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (email == null) return Unauthorized();

        try
        {
            await _addressService.DeleteAddressAsync(email, id);
            return Ok(new { message = "Address deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting address", error = ex.Message });
        }
    }
}