using GLMS.Web_POE.Data;
using GLMS.Web_POE.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web_POE.Api.Repositories
{
	public class ContractRepository : IContractRepository
	{
		private readonly ApplicationDbContext _context;

		public ContractRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<Contract?> GetByIdAsync(int id)
		{
			return await _context.Contracts
				.Include(c => c.Client)
				.FirstOrDefaultAsync(c => c.Id == id);
		}

		public async Task<IEnumerable<Contract>> GetAllAsync()
		{
			return await _context.Contracts
				.Include(c => c.Client)
				.ToListAsync();
		}

		public async Task<Contract> AddAsync(Contract entity)
		{
			_context.Contracts.Add(entity);
			await SaveChangesAsync();
			return entity;
		}

		public async Task<Contract> UpdateAsync(Contract entity)
		{
			_context.Contracts.Update(entity);
			await SaveChangesAsync();
			return entity;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var contract = await _context.Contracts.FindAsync(id);
			if (contract == null)
				return false;

			_context.Contracts.Remove(contract);
			await SaveChangesAsync();
			return true;
		}

		public async Task SaveChangesAsync()
		{
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<Contract>> GetFilteredAsync(int? status, int? clientId, DateTime? startDate, DateTime? endDate)
		{
			var query = _context.Contracts.Include(c => c.Client).AsQueryable();

			if (status.HasValue)
				query = query.Where(c => (int)c.Status == status.Value);

			if (clientId.HasValue)
				query = query.Where(c => c.ClientId == clientId.Value);

			if (startDate.HasValue)
				query = query.Where(c => c.StartDate >= startDate.Value);

			if (endDate.HasValue)
				query = query.Where(c => c.EndDate <= endDate.Value);

			return await query.ToListAsync();
		}

		public async Task<Contract?> GetWithClientAsync(int id)
		{
			return await _context.Contracts
				.Include(c => c.Client)
				.FirstOrDefaultAsync(c => c.Id == id);
		}

		public async Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId)
		{
			return await _context.Contracts
				.Where(c => c.ClientId == clientId)
				.ToListAsync();
		}
	}
}
