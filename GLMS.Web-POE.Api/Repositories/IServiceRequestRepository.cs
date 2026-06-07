using GLMS.Web_POE.Models;

namespace GLMS.Web_POE.Api.Repositories
{
	public interface IServiceRequestRepository : IRepository<ServiceRequest>
	{
		Task<IEnumerable<ServiceRequest>> GetByContractIdAsync(int contractId);
		Task<IEnumerable<ServiceRequest>> GetFilteredAsync(int? contractId, int? status);
	}
}
