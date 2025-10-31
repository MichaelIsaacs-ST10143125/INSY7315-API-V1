using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace NewDawnPropertiesApi_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            // Optional: Simulate async work or warm up resources
            await Task.Delay(50);

            return Ok(new
            {
                status = "ok",
                message = "API is awake and ready.",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
