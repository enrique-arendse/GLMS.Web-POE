namespace GLMS.Web_POE.Api.DTOs
{
	public class ServiceRequestDto
	{
		public int Id { get; set; }
		public int ContractId { get; set; }
		public string Description { get; set; } = string.Empty;
		public decimal Cost { get; set; }
		public int Status { get; set; }
		public decimal? AmountUsd { get; set; }
		public decimal? ExchangeRate { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	public class CreateServiceRequestDto
	{
		public int ContractId { get; set; }
		public string Description { get; set; } = string.Empty;
		public decimal Cost { get; set; }
		public decimal? AmountUsd { get; set; }
		public decimal? ExchangeRate { get; set; }
	}

	public class UpdateServiceRequestStatusDto
	{
		public int Status { get; set; }
	}
}
