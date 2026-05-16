using System.ComponentModel.DataAnnotations;

namespace GLMS.Web_POE.Models
{
	public class Contract
	{
		public int Id { get; set; }

		[Required]
		public int ClientId { get; set; }
		public Client? Client { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime StartDate { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime EndDate { get; set; }

		[Required]
		public ContractStatus Status { get; set; }

		[Required]
		[StringLength(100)]
		public string ServiceLevel { get; set; } = string.Empty;

		public string? SignedAgreementFileName { get; set; }
		public string? SignedAgreementFilePath { get; set; }

		public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
	}
}