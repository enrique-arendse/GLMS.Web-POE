using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Web_POE.Models
{
	public class ServiceRequest
	{
		public int Id { get; set; }

		[Required]
		public int ContractId { get; set; }
		public Contract? Contract { get; set; }

		[Required]
		[StringLength(500)]
		public string Description { get; set; } = string.Empty;

		[Column(TypeName = "decimal(18,2)")]
		public decimal Cost { get; set; }

		[Required]
		public ServiceRequestStatus Status { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal? AmountUsd { get; set; }

		[Column(TypeName = "decimal(18,6)")]
		public decimal? ExchangeRate { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}