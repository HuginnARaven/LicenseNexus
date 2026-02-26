using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitMeasureController(IUnitMeasureService unitMeasureService) : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var unitMeasures = await unitMeasureService.GetAllUnitMeasures();
            return Ok(unitMeasures);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var unitMeasure = await unitMeasureService.GetUnitMeasureById(id);
            if (unitMeasure == null)
            {
                return NotFound();
            }
            return Ok(unitMeasure);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] UnitMeasureRequestDto unitMeasure)
        {
            if (unitMeasure == null)
            {
                return BadRequest();
            }
            return Ok(await unitMeasureService.AddUnitMeasure(unitMeasure));
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UnitMeasureRequestDto unitMeasure)
        {
            if (unitMeasure == null)
            {
                return BadRequest();
            }
            await unitMeasureService.UpdateUnitMeasure(id, unitMeasure);
            return Ok();
        }
        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await unitMeasureService.DeleteUnitMeasure(id);
            return NoContent();
        }
    }
}