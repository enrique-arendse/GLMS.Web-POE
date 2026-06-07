using GLMS.Web_POE.Models;

namespace GLMS.Web_POE.Api.Repositories
{
	public interface IContractRepository : IRepository<Contract>
	{
		Task<IEnumerable<Contract>> GetFilteredAsync(int? status, int? clientId, DateTime? startDate, DateTime? endDate);
		Task<Contract?> GetWithClientAsync(int id);
		Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId);
	}
}
