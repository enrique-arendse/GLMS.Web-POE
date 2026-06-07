using GLMS.Web_POE.Models;

namespace GLMS.Web_POE.Api.Repositories
{
	public interface IClientRepository : IRepository<Client>
	{
		Task<Client?> GetByNameAsync(string name);
	}
}
