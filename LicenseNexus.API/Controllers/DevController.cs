using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DevController(IProductRepository productRepository, IVendorService vendorService) : ControllerBase
{
    [HttpGet("product/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpGet("product")]
    public async Task<IActionResult> GetAll()
    {
        var products = await productRepository.GetAllAsync();
        return Ok(products);
    }

    [HttpPost("product")]
    public async Task<IActionResult> Create([FromBody] ProductModel product)
    {
        if (product == null)
        {
            return BadRequest();
        }
        await productRepository.AddAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("product/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductModel product)
    {
        if (product == null || id != product.Id)
        {
            return BadRequest();
        }
        
        var existingProduct = await productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        await productRepository.UpdateAsync(product);
        return NoContent();
    }

    [HttpDelete("product/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existingProduct = await productRepository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        await productRepository.DeleteAsync(id);
        return NoContent();
    }
}