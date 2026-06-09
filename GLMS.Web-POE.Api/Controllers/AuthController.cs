using GLMS.Web_POE.Api.DTOs;
using GLMS.Web_POE.Api.DTOs.GLMS.Web_POE.Api.DTOs;
using GLMS.Web_POE.Data;
using GLMS.Web_POE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
		private readonly ApplicationDbContext _context;
		private readonly IConfiguration _configuration;

		public AuthController(ApplicationDbContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
		}

		// POST /api/auth/register
		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var emailExists = await _context.Users
				.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

			if (emailExists)
				return Conflict(new { message = "A user with this email already exists." });

			var usernameExists = await _context.Users
				.AnyAsync(u => u.Username.ToLower() == dto.Username.ToLower());

			if (usernameExists)
				return Conflict(new { message = "This username is already taken." });

			var user = new User
			{
				Username = dto.Username,
				Email = dto.Email,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
				CreatedAt = DateTime.UtcNow
			};

			_context.Users.Add(user);
			await _context.SaveChangesAsync();

			return Ok(new { message = "Registration successful." });
		}

		// POST /api/auth/login
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var user = await _context.Users
				.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

			if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
				return Unauthorized(new { message = "Invalid email or password." });

			var token = GenerateJwtToken(user);

			return Ok(new AuthResponseDto
			{
				Token = token,
				Username = user.Username,
				Email = user.Email
			});
		}

		private string GenerateJwtToken(User user)
		{
			var jwtSettings = _configuration.GetSection("JwtSettings");
			var key = Encoding.ASCII.GetBytes(
				jwtSettings["SecretKey"] ?? "default-secret-key-for-development-only-change-this"
			);

			var claims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.Username),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim("role", "Admin")
			};

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddHours(8),
				SigningCredentials = new SigningCredentials(
					new SymmetricSecurityKey(key),
					SecurityAlgorithms.HmacSha256Signature
				)
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
	}
}