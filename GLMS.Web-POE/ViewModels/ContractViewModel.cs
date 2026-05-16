using GLMS.Web_POE.Models;
using System.ComponentModel.DataAnnotations;

namespace GLMS.Web_POE.ViewModels
{
	public class ContractViewModel
	{
		public int Id { get; set; }

		[Required]
		public int ClientId { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime StartDate { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime EndDate { get; set; }

		[Required]
		public ContractStatus Status { get; set; }

		[Required]
		public string ServiceLevel { get; set; } = string.Empty;

		public IFormFile? SignedAgreement { get; set; }
	}
}