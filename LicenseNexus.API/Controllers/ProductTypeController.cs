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
            await productTypeService.AddProductType(productType);
            return Ok();
        }
    }
}