using GLMS.Web_POE.Models;

namespace GLMS.Web_POE.Services
{
	public interface IServiceRequestService
	{
		Task<ServiceRequest> CreateAsync(int contractId, string description, decimal amountUsd);
	}
}
