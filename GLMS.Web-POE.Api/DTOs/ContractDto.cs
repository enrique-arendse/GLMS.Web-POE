namespace GLMS.Web_POE.Api.DTOs
{
	public class ContractDto
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public string? ClientName { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string ServiceLevel { get; set; } = string.Empty;
		public string? SignedAgreementFileName { get; set; }
	}

	public class CreateContractDto
	{
		public int ClientId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string ServiceLevel { get; set; } = string.Empty;
	}

	public class UpdateContractStatusDto
	{
		public int Status { get; set; }
	}
}
