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
			try
			{
				var url = "https://open.er-api.com/v6/latest/USD";

				using var response = await _httpClient.GetAsync(url);
				response.EnsureSuccessStatusCode();

				var json = await response.Content.ReadAsStringAsync();

				using var document = JsonDocument.Parse(json);

				return document.RootElement
					.GetProperty("rates")
					.GetProperty("ZAR")
					.GetDecimal();
			}
			catch
			{
				return 18.50m;
			}
		}

		public decimal ConvertUsdToZar(decimal usdAmount, decimal exchangeRate)
		{
			return Math.Round(usdAmount * exchangeRate, 2);
		}
	}
}