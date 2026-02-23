using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController(ITagService tagService) : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var tags = await tagService.GetAllTags();
            return Ok(tags);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tag = await tagService.GetTagById(id);
            if (tag == null)
            {
                return NotFound();
            }
            return Ok(tag);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] TagRequestDto tag)
        {
            if (tag == null)
            {
                return BadRequest();
            }
            return Ok(await tagService.AddTag(tag));
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TagRequestDto tag)
        {
            if (tag == null)
            {
                return BadRequest();
            }
            await tagService.UpdateTag(id, tag);
            return Ok();
        }
    }
}