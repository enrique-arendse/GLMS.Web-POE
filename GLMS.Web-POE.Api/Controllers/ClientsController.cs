using GLMS.Web_POE.Api.DTOs;
using GLMS.Web_POE.Api.Repositories;
using GLMS.Web_POE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web_POE.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;

        public ClientsController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
        {
            var clients = await _clientRepository.GetAllAsync();
            return Ok(clients.Select(MapToDto).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDto>> GetClient(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);

            if (client == null)
                return NotFound(new { message = $"Client with ID {id} not found" });

            return Ok(MapToDto(client));
        }

        [HttpPost]
        public async Task<ActionResult<ClientDto>> CreateClient([FromBody] CreateClientDto createClientDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = new Client
            {
                Name = createClientDto.Name,
                ContactDetails = createClientDto.ContactDetails,
                Region = createClientDto.Region
            };

            var created = await _clientRepository.AddAsync(client);
            return CreatedAtAction(nameof(GetClient), new { id = created.Id }, MapToDto(created));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ClientDto>> UpdateClient(int id, [FromBody] UpdateClientDto updateClientDto)
        {
            var client = await _clientRepository.GetByIdAsync(id);

            if (client == null)
                return NotFound(new { message = $"Client with ID {id} not found" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            client.Name = updateClientDto.Name;
            client.ContactDetails = updateClientDto.ContactDetails;
            client.Region = updateClientDto.Region;

            var updated = await _clientRepository.UpdateAsync(client);
            return Ok(MapToDto(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var deleted = await _clientRepository.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = $"Client with ID {id} not found" });

            return NoContent();
        }

        private static ClientDto MapToDto(Client client) => new()
        {
            Id = client.Id,
            Name = client.Name,
            ContactDetails = client.ContactDetails,
            Region = client.Region
        };
    }
}
