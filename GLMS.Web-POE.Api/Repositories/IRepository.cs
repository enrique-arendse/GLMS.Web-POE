using GLMS.Web_POE.Models;

namespace GLMS.Web_POE.Api.Repositories
{
	public interface IRepository<T> where T : class
	{
		Task<T?> GetByIdAsync(int id);
		Task<IEnumerable<T>> GetAllAsync();
		Task<T> AddAsync(T entity);
		Task<T> UpdateAsync(T entity);
		Task<bool> DeleteAsync(int id);
		Task SaveChangesAsync();
	}
}
