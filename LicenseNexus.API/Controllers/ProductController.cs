using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Models;
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
        
        [HttpGet("catalog")]
        public async Task<IActionResult> GetPaginated([FromQuery] ProductFilterDto filter)
        {
            var products = await productService.GetPaginatedAsync(filter);
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductRequestDto product)
        {
            if (product == null)
            {
                return BadRequest();
            }
            
            return Ok(await productService.AddAsync(product));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequestDto product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            await productService.UpdateAsync(id, product);
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromBody] ProductPatchFields updates)
        {
            if (updates == null)
            {
                return BadRequest();
            }

            await productService.PatchAsync(id, updates);
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
        
        [HttpPost("{productId}/price")]
        public async Task<IActionResult> AddPrice(int productId, [FromBody] ProductPriceRequestDto priceDto)
        {
            if (priceDto == null)
            {
                return BadRequest();
            }

            var result = await productService.AddProductPrice(productId, priceDto);
            if (result == null)
            {
                return BadRequest("Could not add price to the product.");
            }

            return Ok(result);
        }

        [HttpPut("{productId}/price/{priceId}")]
        public async Task<IActionResult> UpdatePrice(int productId, int priceId, [FromBody] ProductPriceRequestDto priceDto)
        {
            if (priceDto == null)
            {
                return BadRequest();
            }

            await productService.UpdateProductPrice(productId, priceId, priceDto);
            return NoContent();
        }

        [HttpDelete("{productId}/price/{priceId}")]
        public async Task<IActionResult> DeletePrice(int productId, int priceId)
        {
            await productService.DeleteProductPrice(productId, priceId);
            return NoContent();
        }
        
        [HttpPost("{productId}/tag/{tagId}")]
        public async Task<IActionResult> AddTag(int productId, int tagId)
        {
            await productService.AddProductTag(productId, tagId);
            return Ok();
        }

        [HttpDelete("{productId}/tag/{tagId}")]
        public async Task<IActionResult> DeleteTag(int productId, int tagId)
        {
            await productService.DeleteProductTag(productId, tagId);
            return NoContent();
        }

    }
}