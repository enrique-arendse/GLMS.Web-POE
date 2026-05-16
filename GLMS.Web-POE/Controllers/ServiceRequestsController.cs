using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Web_POE.Data;
using GLMS.Web_POE.Models;
using GLMS.Web_POE.Services;

namespace GLMS.Web_POE.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ServiceRequests.Include(s => s.Contract);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create()
        {
            var contracts = await _context.Contracts
                .Where(c => c.Status != ContractStatus.Expired && c.Status != ContractStatus.OnHold)
                .ToListAsync();

            ViewData["ContractId"] = new SelectList(contracts, "Id", "ServiceLevel");
            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ContractId,Description,AmountUsd")] ServiceRequest serviceRequest)
        {
            var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError("ContractId", "Contract not found.");
            }
            else if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
            {
                ModelState.AddModelError("ContractId", "Cannot create a service request for an Expired or On Hold contract.");
            }

            if (!ModelState.IsValid)
            {
                var contracts = await _context.Contracts
                    .Where(c => c.Status != ContractStatus.Expired && c.Status != ContractStatus.OnHold)
                    .ToListAsync();
                ViewData["ContractId"] = new SelectList(contracts, "Id", "ServiceLevel", serviceRequest.ContractId);
                return View(serviceRequest);
            }

            try
            {
                var exchangeRate = await GetExchangeRateAsync();
				var zarCost = Math.Round((serviceRequest.AmountUsd ?? 0) * exchangeRate, 2);

				serviceRequest.ExchangeRate = exchangeRate;
                serviceRequest.Cost = zarCost;
                serviceRequest.Status = ServiceRequestStatus.Pending;
                serviceRequest.CreatedAt = DateTime.UtcNow;

                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the service request: " + ex.Message);
            }

            var contractsForView = await _context.Contracts
                .Where(c => c.Status != ContractStatus.Expired && c.Status != ContractStatus.OnHold)
                .ToListAsync();
            ViewData["ContractId"] = new SelectList(contractsForView, "Id", "ServiceLevel", serviceRequest.ContractId);
            return View(serviceRequest);
        }

        private async Task<decimal> GetExchangeRateAsync()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://open.er-api.com/v6/latest/USD");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            using var document = System.Text.Json.JsonDocument.Parse(content);
            var rate = document.RootElement
                .GetProperty("rates")
                .GetProperty("ZAR")
                .GetDecimal();

            return rate;
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceLevel", serviceRequest.ContractId);
            return View(serviceRequest);
        }

		// POST: ServiceRequests/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,ContractId,Description,Status,AmountUsd,CreatedAt")] ServiceRequest serviceRequest)
		{
			if (id != serviceRequest.Id) return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					var exchangeRate = await GetExchangeRateAsync();
					serviceRequest.ExchangeRate = exchangeRate;
					serviceRequest.Cost = Math.Round((serviceRequest.AmountUsd ?? 0) * exchangeRate, 2);

					_context.Update(serviceRequest);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ServiceRequestExists(serviceRequest.Id))
						return NotFound();
					else
						throw;
				}
			}

			ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "ServiceLevel", serviceRequest.ContractId);
			return View(serviceRequest);
		}

		// GET: ServiceRequests/Delete/5
		public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // POST: ServiceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest != null)
            {
                _context.ServiceRequests.Remove(serviceRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests.Any(e => e.Id == id);
        }
    }
}
