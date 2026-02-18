using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IProductService productService) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await productService.GetAllAsync();
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductRequestDTO product)
        {
            if (product == null)
            {
                return BadRequest();
            }
            await productService.AddAsync(product);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequestDTO product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            var existingProduct = await productService.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            await productService.UpdateAsync(id, product);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingProduct = await productService.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            await productService.DeleteAsync(id);
            return NoContent();
        }
    }
}
