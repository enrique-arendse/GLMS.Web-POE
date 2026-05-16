using System.Text.Json;

namespace GLMS.Web_POE.Services
{
	public class CurrencyService : ICurrencyService
	{
		private readonly HttpClient _httpClient;

		public CurrencyService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<decimal> GetUsdToZarRateAsync()
		{
			var url = "https://open.er-api.com/v6/latest/USD";

			using var response = await _httpClient.GetAsync(url);
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();

			using var document = JsonDocument.Parse(json);

			var rate = document.RootElement
				.GetProperty("rates")
				.GetProperty("ZAR")
				.GetDecimal();

			return rate;
		}

		public decimal ConvertUsdToZar(decimal usdAmount, decimal exchangeRate)
		{
			return Math.Round(usdAmount * exchangeRate, 2);
		}
	}
}