using GLMS.Web_POE.Models;
using GLMS.Web_POE.Services;
using GLMS.Web_POE.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web_POE.Controllers
{
	public class ServiceRequestsController : Controller
	{
		private readonly IServiceRequest _serviceRequestService;
		private readonly IApiContractService _apiContractService;
		private readonly ICurrencyService _currencyService;
		private readonly ILogger<ServiceRequestsController> _logger;

		public ServiceRequestsController(
			IServiceRequest serviceRequestService,
			IApiContractService apiContractService,
			ICurrencyService currencyService,
			ILogger<ServiceRequestsController> logger)
		{
			_serviceRequestService = serviceRequestService;
			_apiContractService = apiContractService;
			_currencyService = currencyService;
			_logger = logger;
		}

		// GET: ServiceRequests
		public async Task<IActionResult> Index()
		{
			try
			{
				var requests = await _serviceRequestService.GetAllAsync();
				return View(requests);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error loading service requests: {ex.Message}");
				ModelState.AddModelError("", "Error loading service requests");
				return View(new List<ServiceRequest>());
			}
		}

		// GET: ServiceRequests/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return NotFound();

			try
			{
				var request = await _serviceRequestService.GetByIdAsync(id.Value);

				if (request == null)
					return NotFound();

				return View(request);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error loading service request: {ex.Message}");
				return NotFound();
			}
		}

		// GET: ServiceRequests/Create
		public async Task<IActionResult> Create()
		{
			try
			{
				var contracts = await _apiContractService.GetContractsAsync();
				var rate = await _currencyService.GetUsdToZarRateAsync();

				var model = new ServiceRequestCreateViewModel
				{
					CurrentRate = rate
				};

				ViewData["ContractId"] = BuildContractSelectList(contracts);

				return View(model);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error loading create page: {ex.Message}");
				return View(new ServiceRequestCreateViewModel());
			}
		}

		// POST: ServiceRequests/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			[Bind("Id,ContractId,Description,AmountUsd")]
			ServiceRequest serviceRequest)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var exchangeRate = await _currencyService.GetUsdToZarRateAsync();
					var amountUsd = serviceRequest.AmountUsd ?? 0;
					var cost = _currencyService.ConvertUsdToZar(amountUsd, exchangeRate);

					await _serviceRequestService.CreateAsync(
						serviceRequest.ContractId,
						serviceRequest.Description,
						amountUsd,
						exchangeRate,
						cost);
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error creating service request: {ex.Message}");
					ModelState.AddModelError("", "Error creating service request");
				}
			}

			var contracts = await _apiContractService.GetContractsAsync();

			ViewData["ContractId"] = BuildContractSelectList(contracts, serviceRequest.ContractId);

			return View(serviceRequest);
		}

		// GET: ServiceRequests/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
				return NotFound();

			try
			{
				var request = await _serviceRequestService.GetByIdAsync(id.Value);

				if (request == null)
					return NotFound();

				var contracts = await _apiContractService.GetContractsAsync();

				ViewData["ContractId"] = BuildContractSelectList(contracts, request.ContractId);

				return View(request);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error loading service request: {ex.Message}");
				return NotFound();
			}
		}

		// POST: ServiceRequests/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(
			int id,
			[Bind("Id,ContractId,Description,Status,AmountUsd,CreatedAt")]
			ServiceRequest serviceRequest)
		{
			if (id != serviceRequest.Id)
				return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					await _serviceRequestService.UpdateAsync(id, serviceRequest);
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error updating service request: {ex.Message}");
					ModelState.AddModelError("", "Error updating service request");
				}
			}

			var contracts = await _apiContractService.GetContractsAsync();

			ViewData["ContractId"] = BuildContractSelectList(contracts, serviceRequest.ContractId);

			return View(serviceRequest);
		}

		private static SelectList BuildContractSelectList(IEnumerable<Contract> contracts, int? selectedId = null)
		{
			var items = contracts.Select(c => new
			{
				c.Id,
				DisplayName = c.Client?.Name ?? $"Contract #{c.Id}"
			});

			return new SelectList(items, "Id", "DisplayName", selectedId);
		}

		// GET: ServiceRequests/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
				return NotFound();

			try
			{
				var request = await _serviceRequestService.GetByIdAsync(id.Value);

				if (request == null)
					return NotFound();

				return View(request);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error loading service request: {ex.Message}");
				return NotFound();
			}
		}

		// POST: ServiceRequests/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			try
			{
				await _serviceRequestService.DeleteAsync(id);
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error deleting service request: {ex.Message}");
				return RedirectToAction(nameof(Index));
			}
		}
	}
}