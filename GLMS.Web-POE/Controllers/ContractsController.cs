using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GLMS.Web_POE.Data;
using GLMS.Web_POE.Models;
using GLMS.Web_POE.Services;
using GLMS.Web_POE.ViewModels;

namespace GLMS.Web_POE.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IApiContractService _apiContractService;
        private readonly IApiClientService _apiClientService;
        private readonly IFileService _fileService;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(
            IApiContractService apiContractService,
            IApiClientService apiClientService,
            IFileService fileService,
            ILogger<ContractsController> logger)
        {
            _apiContractService = apiContractService;
            _apiClientService = apiClientService;
            _fileService = fileService;
            _logger = logger;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? status)
        {
            try
            {
                var contracts = await _apiContractService.GetContractsAsync(status, null, startDate, endDate);
                ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
                ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");
                ViewData["CurrentStatus"] = status;
                ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(ContractStatus)).Cast<ContractStatus>());
                return View(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading contracts: {ex.Message}");
                ModelState.AddModelError("", "Error loading contracts from API");
                return View(new List<Contract>());
            }
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var contract = await _apiContractService.GetContractAsync(id.Value);
                if (contract == null)
                    return NotFound();
                return View(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading contract: {ex.Message}");
                return NotFound();
            }
        }

        // GET: Contracts/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var clients = await _apiClientService.GetClientsAsync();
                ViewData["ClientId"] = new SelectList(clients, "Id", "Name");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading clients: {ex.Message}");
                ModelState.AddModelError("", "Error loading clients from API");
                return View();
            }
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract, IFormFile? signedAgreement)
        {
            if (ModelState.IsValid)
            {
                try
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
                            var clients = await _apiClientService.GetClientsAsync();
                            ViewData["ClientId"] = new SelectList(clients, "Id", "Name", contract.ClientId);
                            return View(contract);
                        }
                    }

                    await _apiContractService.CreateContractAsync(contract);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error creating contract: {ex.Message}");
                    ModelState.AddModelError("", "Error creating contract");
                    var clients = await _apiClientService.GetClientsAsync();
                    ViewData["ClientId"] = new SelectList(clients, "Id", "Name", contract.ClientId);
                    return View(contract);
                }
            }

            try
            {
                var clients = await _apiClientService.GetClientsAsync();
                ViewData["ClientId"] = new SelectList(clients, "Id", "Name", contract.ClientId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading clients: {ex.Message}");
            }
            return View(contract);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var contract = await _apiContractService.GetContractAsync(id.Value);
                if (contract == null)
                    return NotFound();

                var clients = await _apiClientService.GetClientsAsync();
                ViewData["ClientId"] = new SelectList(clients, "Id", "Name", contract.ClientId);
                return View(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading contract: {ex.Message}");
                return NotFound();
            }
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract, IFormFile? signedAgreement)
        {
            if (id != contract.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingContract = await _apiContractService.GetContractAsync(id);
                    if (existingContract == null)
                        return NotFound();

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
                            var clients = await _apiClientService.GetClientsAsync();
                            ViewData["ClientId"] = new SelectList(clients, "Id", "Name", contract.ClientId);
                            return View(contract);
                        }
                    }

                    // Update status only through PATCH endpoint for status changes
                    if (contract.Status != existingContract.Status)
                    {
                        await _apiContractService.UpdateContractStatusAsync(id, (int)contract.Status);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating contract: {ex.Message}");
                    ModelState.AddModelError("", "Error updating contract");
                }
            }

            try
            {
                var clients = await _apiClientService.GetClientsAsync();
                ViewData["ClientId"] = new SelectList(clients, "Id", "Name", contract.ClientId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading clients: {ex.Message}");
            }
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var contract = await _apiContractService.GetContractAsync(id.Value);
                if (contract == null)
                    return NotFound();
                return View(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading contract: {ex.Message}");
                return NotFound();
            }
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _apiContractService.DeleteContractAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting contract: {ex.Message}");
                ModelState.AddModelError("", "Error deleting contract");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Contracts/DownloadAgreement/5
        public async Task<IActionResult> DownloadAgreement(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var contract = await _apiContractService.GetContractAsync(id.Value);
                if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementFilePath))
                    return NotFound();

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.SignedAgreementFilePath);

                if (!System.IO.File.Exists(filePath))
                    return NotFound();

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/pdf", contract.SignedAgreementFileName ?? "agreement.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading agreement: {ex.Message}");
                return NotFound();
            }
        }
    }
}
