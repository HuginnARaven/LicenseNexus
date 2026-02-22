using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using Microsoft.AspNetCore.Http;
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
            return Ok(await service.GetAllAsync());
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Partner?>> Get(int id)
        {
            var order = await service.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
        
        [HttpPost]
        public async Task<ActionResult<Partner?>> Post([FromBody] PartnerRequestDto partner)
        {
            return Ok(await service.CreateAsync(partner));
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] PartnerRequestDto partner)
        {
            await service.UpdateAsync(id, partner);
            return Ok();
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await service.DeleteAsync(id);
            return Ok();
        }
    }
}
