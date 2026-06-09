namespace GLMS.Web_POE.Api.DTOs
{
	namespace GLMS.Web_POE.Api.DTOs
	{
		public class RegisterDto
		{
			public string Username { get; set; } = string.Empty;
			public string Email { get; set; } = string.Empty;
			public string Password { get; set; } = string.Empty;
		}

		public class LoginDto
		{
			public string Email { get; set; } = string.Empty;
			public string Password { get; set; } = string.Empty;
		}

		public class AuthResponseDto
		{
			public string Token { get; set; } = string.Empty;
			public string Username { get; set; } = string.Empty;
			public string Email { get; set; } = string.Empty;
		}
	}
}
