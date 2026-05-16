namespace GLMS.Web_POE.Services
{
	public interface ICurrencyService
	{
		Task<decimal> GetUsdToZarRateAsync();
		decimal ConvertUsdToZar(decimal usdAmount, decimal exchangeRate);
	}
}
