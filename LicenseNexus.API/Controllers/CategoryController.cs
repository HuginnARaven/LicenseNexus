using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryService categoryService) : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await categoryService.GetAllCategories();
            return Ok(categories);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await categoryService.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] CategoryRequestDto category)
        {
            if (category == null)
            {
                return BadRequest();
            }
            await categoryService.AddCategory(category);
            return Ok();
        }
    }
}