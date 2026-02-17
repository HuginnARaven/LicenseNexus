using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductGroupController(IProductGroupService productGroupService) : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var groups = await productGroupService.GetAllProductGroups();
            return Ok(groups);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var group = await productGroupService.GetProductGroupById(id);
            if (group == null)
            {
                return NotFound();
            }
            return Ok(group);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] ProductGroupRequestDTO group)
        {
            if (group == null)
            {
                return BadRequest();
            }
            await productGroupService.AddProductGroup(group);
            return Ok();
        }
    }
}