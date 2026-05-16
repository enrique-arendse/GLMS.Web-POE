using System.ComponentModel.DataAnnotations;

namespace GLMS.Web_POE.ViewModels
{
	public class ServiceRequestCreateViewModel
	{
		[Required]
		public int ContractId { get; set; }

		[Required]
		[StringLength(500)]
		public string Description { get; set; } = string.Empty;

		[Required]
		[Range(0.01, 1000000)]
		public decimal AmountUsd { get; set; }

		public decimal? CurrentRate { get; set; }
		public decimal? CalculatedZarCost { get; set; }
	}
}
