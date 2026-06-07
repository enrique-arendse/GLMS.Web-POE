using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web_POE.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class HealthController : ControllerBase
	{
		[HttpGet]
		public IActionResult GetHealth()
		{
			return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
		}
	}
}
