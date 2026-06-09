using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace GLMS.Web_POE.Services
{
	public class ApiAuthTokenHandler : DelegatingHandler
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ApiAuthTokenHandler> _logger;
		private string? _cachedToken;
		private readonly SemaphoreSlim _tokenLock = new(1, 1);

		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNameCaseInsensitive = true
		};

		public ApiAuthTokenHandler(IHttpClientFactory httpClientFactory, ILogger<ApiAuthTokenHandler> logger)
		{
			_httpClientFactory = httpClientFactory;
			_logger = logger;
		}

		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			var token = await GetTokenAsync(cancellationToken);
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var response = await base.SendAsync(request, cancellationToken);

			if (response.StatusCode != HttpStatusCode.Unauthorized)
				return response;

			response.Dispose();
			_cachedToken = null;

			token = await GetTokenAsync(cancellationToken);
			using var retryRequest = await CloneRequestAsync(request, cancellationToken);
			retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			return await base.SendAsync(retryRequest, cancellationToken);
		}

		private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
		{
			if (!string.IsNullOrEmpty(_cachedToken))
				return _cachedToken;

			await _tokenLock.WaitAsync(cancellationToken);
			try
			{
				if (!string.IsNullOrEmpty(_cachedToken))
					return _cachedToken;

				var authClient = _httpClientFactory.CreateClient("ApiAuth");
				var response = await authClient.PostAsync("api/auth/token?username=admin", null, cancellationToken);

				if (!response.IsSuccessStatusCode)
				{
					var body = await response.Content.ReadAsStringAsync(cancellationToken);
					_logger.LogError("Failed to obtain API token. Status: {StatusCode}, Body: {Body}",
						response.StatusCode, body);
					response.EnsureSuccessStatusCode();
				}

				var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions, cancellationToken);
				if (string.IsNullOrWhiteSpace(tokenResponse?.Token))
					throw new InvalidOperationException("API returned an empty authentication token.");

				_cachedToken = tokenResponse.Token;
				return _cachedToken;
			}
			finally
			{
				_tokenLock.Release();
			}
		}

		private static async Task<HttpRequestMessage> CloneRequestAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			var clone = new HttpRequestMessage(request.Method, request.RequestUri);

			if (request.Content != null)
			{
				var stream = new MemoryStream();
				await request.Content.CopyToAsync(stream, cancellationToken);
				stream.Position = 0;
				clone.Content = new StreamContent(stream);

				foreach (var header in request.Content.Headers)
					clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			foreach (var header in request.Headers)
			{
				if (!header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
					clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			return clone;
		}

		private sealed class TokenResponse
		{
			public string Token { get; set; } = string.Empty;
		}
	}
}
