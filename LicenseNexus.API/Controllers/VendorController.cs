using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorController(IVendorService vendorService) : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var products = await vendorService.GetAllVendors();
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vendor = await vendorService.GetVendorById(id);
            if (vendor == null)
            {
                return NotFound();
            }
            return Ok(vendor);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] VendorRequestDTO vendor)
        {
            if (vendor == null)
            {
                return BadRequest();
            }
            return Ok(await vendorService.AddVendor(vendor));
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] VendorRequestDTO vendor)
        {
            if (vendor == null)
            {
                return BadRequest();
            }
            await vendorService.UpdateVendor(id, vendor);
            return Ok();
        }
        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await vendorService.DeleteVendor(id);
            return NoContent();
        }
    }
}
