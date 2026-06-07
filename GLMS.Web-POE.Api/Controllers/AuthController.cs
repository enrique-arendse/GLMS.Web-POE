using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GLMS.Web_POE.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public AuthController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		/// <summary>
		/// Generate JWT token for API access
		/// </summary>
		/// <param name="username">Username for token</param>
		/// <returns>JWT token</returns>
		[HttpPost("token")]
		public IActionResult GetToken([FromQuery] string username = "admin")
		{
			if (string.IsNullOrEmpty(username))
			{
				return BadRequest(new { message = "Username is required" });
			}

			var jwtSettings = _configuration.GetSection("JwtSettings");
			var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
				jwtSettings["SecretKey"] ?? "default-secret-key-for-development-only-change-in-production"));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, username),
				new Claim(ClaimTypes.Name, username),
				new Claim("role", "Admin")
			};

			var token = new JwtSecurityToken(
				issuer: jwtSettings["Issuer"],
				audience: jwtSettings["Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"] ?? "60")),
				signingCredentials: credentials);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

			return Ok(new { token = tokenString });
		}
	}
}
