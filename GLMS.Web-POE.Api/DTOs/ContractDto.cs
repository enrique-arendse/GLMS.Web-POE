namespace GLMS.Web_POE.Api.DTOs
{
	public class ContractDto
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public string? ClientName { get; set; }
		public string? ClientRegion { get; set; }           
		public string? ClientContactDetails { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string ServiceLevel { get; set; } = string.Empty;
		public string? SignedAgreementFileName { get; set; }
		public string? SignedAgreementFilePath { get; set; }
	}

	public class CreateContractDto
	{
		public int ClientId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string ServiceLevel { get; set; } = string.Empty;
		public string? SignedAgreementFileName { get; set; }
		public string? SignedAgreementFilePath { get; set; }
	}

	public class UpdateContractDto
	{
		public int ClientId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string ServiceLevel { get; set; } = string.Empty;
		public string? SignedAgreementFileName { get; set; }
		public string? SignedAgreementFilePath { get; set; }
	}

	public class UpdateContractStatusDto
	{
		public int Status { get; set; }
	}
}
