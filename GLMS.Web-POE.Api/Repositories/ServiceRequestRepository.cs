using GLMS.Web_POE.Data;
using GLMS.Web_POE.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web_POE.Api.Repositories
{
	public class ServiceRequestRepository : IServiceRequestRepository
	{
		private readonly ApplicationDbContext _context;

		public ServiceRequestRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<ServiceRequest?> GetByIdAsync(int id)
		{
			return await _context.ServiceRequests.FindAsync(id);
		}

		public async Task<IEnumerable<ServiceRequest>> GetAllAsync()
		{
			return await _context.ServiceRequests.ToListAsync();
		}

		public async Task<ServiceRequest> AddAsync(ServiceRequest entity)
		{
			_context.ServiceRequests.Add(entity);
			await SaveChangesAsync();
			return entity;
		}

		public async Task<ServiceRequest> UpdateAsync(ServiceRequest entity)
		{
			_context.ServiceRequests.Update(entity);
			await SaveChangesAsync();
			return entity;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var serviceRequest = await _context.ServiceRequests.FindAsync(id);
			if (serviceRequest == null)
				return false;

			_context.ServiceRequests.Remove(serviceRequest);
			await SaveChangesAsync();
			return true;
		}

		public async Task SaveChangesAsync()
		{
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<ServiceRequest>> GetByContractIdAsync(int contractId)
		{
			return await _context.ServiceRequests
				.Where(sr => sr.ContractId == contractId)
				.ToListAsync();
		}

		public async Task<IEnumerable<ServiceRequest>> GetFilteredAsync(int? contractId, int? status)
		{
			var query = _context.ServiceRequests.AsQueryable();

			if (contractId.HasValue)
				query = query.Where(sr => sr.ContractId == contractId.Value);

			if (status.HasValue)
				query = query.Where(sr => (int)sr.Status == status.Value);

			return await query.ToListAsync();
		}
	}
}
