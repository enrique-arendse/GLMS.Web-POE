using GLMS.Web_POE.Data;
using GLMS.Web_POE.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web_POE.Api.Repositories
{
	public class ClientRepository : IClientRepository
	{
		private readonly ApplicationDbContext _context;

		public ClientRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<Client?> GetByIdAsync(int id)
		{
			return await _context.Clients.FindAsync(id);
		}

		public async Task<IEnumerable<Client>> GetAllAsync()
		{
			return await _context.Clients.ToListAsync();
		}

		public async Task<Client> AddAsync(Client entity)
		{
			_context.Clients.Add(entity);
			await SaveChangesAsync();
			return entity;
		}

		public async Task<Client> UpdateAsync(Client entity)
		{
			_context.Clients.Update(entity);
			await SaveChangesAsync();
			return entity;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var client = await _context.Clients.FindAsync(id);
			if (client == null)
				return false;

			_context.Clients.Remove(client);
			await SaveChangesAsync();
			return true;
		}

		public async Task SaveChangesAsync()
		{
			await _context.SaveChangesAsync();
		}

		public async Task<Client?> GetByNameAsync(string name)
		{
			return await _context.Clients.FirstOrDefaultAsync(c => c.Name == name);
		}
	}
}
