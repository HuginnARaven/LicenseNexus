using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypeController(IProductTypeService productTypeService) : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var productTypes = await productTypeService.GetAllProductTypes();
            return Ok(productTypes);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var productType = await productTypeService.GetProductTypeById(id);
            if (productType == null)
            {
                return NotFound();
            }
            return Ok(productType);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] ProductTypeRequestDto productType)
        {
            if (productType == null)
            {
                return BadRequest();
            }
            return Ok(await productTypeService.AddProductType(productType));
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductTypeRequestDto productType)
        {
            if (productType == null)
            {
                return BadRequest();
            }
            await productTypeService.UpdateProductType(id, productType);
            return Ok();
        }
        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await productTypeService.DeleteProductType(id);
            return NoContent();
        }
    }
}