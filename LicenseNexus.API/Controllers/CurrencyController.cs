using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController(ICurrencyService currencyService) : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            var currencies = await currencyService.GetAllCurrencies();
            return Ok(currencies);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currency = await currencyService.GetCurrencyById(id);
            if (currency == null)
            {
                return NotFound();
            }
            return Ok(currency);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] CurrencyRequestDto currency)
        {
            if (currency == null)
            {
                return BadRequest();
            }
            await currencyService.AddCurrency(currency);
            return Ok();
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CurrencyRequestDto currency)
        {
            if (currency == null)
            {
                return BadRequest();
            }
            await currencyService.UpdateCurrency(id, currency);
            return Ok();
        }
    }
}