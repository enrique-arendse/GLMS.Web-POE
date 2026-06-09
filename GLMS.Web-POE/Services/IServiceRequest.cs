using GLMS.Web_POE.Models;

namespace GLMS.Web_POE.Services
{
	public interface IServiceRequest
	{
		Task<List<ServiceRequest>> GetAllAsync();
		Task<ServiceRequest?> GetByIdAsync(int id);
		Task<ServiceRequest> CreateAsync(int contractId, string description, decimal amountUsd, decimal exchangeRate, decimal cost);
		Task UpdateAsync(int id, ServiceRequest serviceRequest);
		Task DeleteAsync(int id);
	}
}