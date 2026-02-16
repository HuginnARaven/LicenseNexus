using Microsoft.AspNetCore.Mvc;

namespace LicenseNexus.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DevController : ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Get()
    {
        return Ok();
    }
}