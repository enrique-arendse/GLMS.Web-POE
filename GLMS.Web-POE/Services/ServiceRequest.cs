using GLMS.Web_POE.Data;
using GLMS.Web_POE.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web_POE.Services
{
	public class ServiceRequestService : IServiceRequestService
	{
		private readonly ApplicationDbContext _context;
		private readonly ICurrencyService _currencyService;
		private readonly IContractService _contractService;

		public ServiceRequestService(
			ApplicationDbContext context,
			ICurrencyService currencyService,
			IContractService contractService)
		{
			_context = context;
			_currencyService = currencyService;
			_contractService = contractService;
		}

		public async Task<ServiceRequest> CreateAsync(int contractId, string description, decimal amountUsd)
		{
			var contract = await _context.Contracts
				.FirstOrDefaultAsync(c => c.Id == contractId);

			if (contract == null)
				throw new InvalidOperationException("Contract not found.");

			if (!_contractService.CanCreateServiceRequest(contract))
				throw new InvalidOperationException("Cannot create a service request for an Expired or On Hold contract.");

			var rate = await _currencyService.GetUsdToZarRateAsync();
			var zarCost = _currencyService.ConvertUsdToZar(amountUsd, rate);

			var serviceRequest = new ServiceRequest
			{
				ContractId = contractId,
				Description = description,
				AmountUsd = amountUsd,
				ExchangeRate = rate,
				Cost = zarCost,
				Status = ServiceRequestStatus.Pending
			};

			_context.ServiceRequests.Add(serviceRequest);
			await _context.SaveChangesAsync();

			return serviceRequest;
		}
	}
}
