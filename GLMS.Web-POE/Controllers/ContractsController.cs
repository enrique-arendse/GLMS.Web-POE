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
using GLMS.Web_POE.ViewModels;

namespace GLMS.Web_POE.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public ContractsController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? status)
        {
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(c => (int)c.Status == status.Value);
            }

            var contracts = await query.ToListAsync();
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentStatus"] = status;
            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(ContractStatus)).Cast<ContractStatus>());

            return View(contracts);
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contracts/Create
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "ContactDetails");
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract, IFormFile? signedAgreement)
        {
            if (ModelState.IsValid)
            {
                if (signedAgreement != null && signedAgreement.Length > 0)
                {
                    try
                    {
                        var (fileName, filePath) = await _fileService.SaveAgreementAsync(signedAgreement);
                        contract.SignedAgreementFileName = fileName;
                        contract.SignedAgreementFilePath = filePath;
                    }
                    catch (InvalidOperationException ex)
                    {
                        ModelState.AddModelError("signedAgreement", ex.Message);
                        ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "ContactDetails", contract.ClientId);
                        return View(contract);
                    }
                }

                _context.Add(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "ContactDetails", contract.ClientId);
            return View(contract);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "ContactDetails", contract.ClientId);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract, IFormFile? signedAgreement)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingContract = await _context.Contracts.FindAsync(id);
                    if (existingContract == null)
                        return NotFound();

                    existingContract.ClientId = contract.ClientId;
                    existingContract.StartDate = contract.StartDate;
                    existingContract.EndDate = contract.EndDate;
                    existingContract.Status = contract.Status;
                    existingContract.ServiceLevel = contract.ServiceLevel;

                    if (signedAgreement != null && signedAgreement.Length > 0)
                    {
                        try
                        {
                            var (fileName, filePath) = await _fileService.SaveAgreementAsync(signedAgreement);
                            existingContract.SignedAgreementFileName = fileName;
                            existingContract.SignedAgreementFilePath = filePath;
                        }
                        catch (InvalidOperationException ex)
                        {
                            ModelState.AddModelError("signedAgreement", ex.Message);
                            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "ContactDetails", contract.ClientId);
                            return View(contract);
                        }
                    }

                    _context.Update(existingContract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "ContactDetails", contract.ClientId);
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }

        // GET: Contracts/DownloadAgreement/5
        public async Task<IActionResult> DownloadAgreement(int? id)
        {
            if (id == null)
                return NotFound();

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementFilePath))
                return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.SignedAgreementFilePath);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", contract.SignedAgreementFileName ?? "agreement.pdf");
        }
    }
}
