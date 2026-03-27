using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController(ICustomerService service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer?>>> Get()
        {
            return Ok(await service.GetAllCustomersAsync());
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer?>> Get(int id)
        {
            return Ok(await service.GetCustomerByIdAsync(id));
        }
        
        [HttpPost]
        public async Task<ActionResult<Customer?>> Post([FromBody] CustomerRequestDto customer)
        {
            return Ok(await service.AddCustomerAsync(customer));
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] CustomerRequestDto customer)
        {
            await service.UpdateCustomerAsync(id, customer);
            return Ok();
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await service.DeleteCustomerAsync(id);
            return Ok();
        }
    }
}
