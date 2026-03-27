using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartnerController(IPartnerService service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            return Ok(await service.GetAllPartnersAsync());
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Partner?>> Get(int id)
        {
            var order = await service.GetPartnerByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
        
        [HttpPost]
        public async Task<ActionResult<Partner?>> Post([FromBody] PartnerRequestDto partner)
        {
            return Ok(await service.CreatePartnerAsync(partner));
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] PartnerRequestDto partner)
        {
            await service.UpdatePartnerAsync(id, partner);
            return Ok();
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await service.DeletePartnerAsync(id);
            return Ok();
        }
    }
}
