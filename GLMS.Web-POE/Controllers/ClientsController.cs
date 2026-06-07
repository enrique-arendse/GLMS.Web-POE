using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GLMS.Web_POE.Models;
using GLMS.Web_POE.Services;

namespace GLMS.Web_POE.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IApiClientService _apiClientService;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(IApiClientService apiClientService, ILogger<ClientsController> logger)
        {
            _apiClientService = apiClientService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var clients = await _apiClientService.GetClientsAsync();
                return View(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading clients: {ex.Message}");
                ModelState.AddModelError("", "Error loading clients from API");
                return View(new List<Client>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var client = await _apiClientService.GetClientAsync(id.Value);
                if (client == null)
                    return NotFound();
                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading client: {ex.Message}");
                return NotFound();
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,ContactDetails,Region")] Client client)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _apiClientService.CreateClientAsync(client);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error creating client: {ex.Message}");
                    ModelState.AddModelError("", "Error creating client");
                }
            }
            return View(client);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var client = await _apiClientService.GetClientAsync(id.Value);
                if (client == null)
                    return NotFound();
                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading client: {ex.Message}");
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContactDetails,Region")] Client client)
        {
            if (id != client.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _apiClientService.UpdateClientAsync(id, client);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating client: {ex.Message}");
                    ModelState.AddModelError("", "Error updating client");
                }
            }
            return View(client);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var client = await _apiClientService.GetClientAsync(id.Value);
                if (client == null)
                    return NotFound();
                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading client: {ex.Message}");
                return NotFound();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _apiClientService.DeleteClientAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting client: {ex.Message}");
                ModelState.AddModelError("", "Error deleting client");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
