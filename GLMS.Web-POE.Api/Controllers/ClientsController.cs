using GLMS.Web_POE.Api.DTOs;
using GLMS.Web_POE.Data;
using GLMS.Web_POE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web_POE.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
        {
            var clients = await _context.Clients.ToListAsync();

            var clientDtos = clients.Select(c => new ClientDto
            {
                Id = c.Id,
                Name = c.Name,
                ContactDetails = c.ContactDetails,
                Region = c.Region
            }).ToList();

            return Ok(clientDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDto>> GetClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
                return NotFound(new { message = $"Client with ID {id} not found" });

            var clientDto = new ClientDto
            {
                Id = client.Id,
                Name = client.Name,
                ContactDetails = client.ContactDetails,
                Region = client.Region
            };

            return Ok(clientDto);
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

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            var clientDto = new ClientDto
            {
                Id = client.Id,
                Name = client.Name,
                ContactDetails = client.ContactDetails,
                Region = client.Region
            };

            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, clientDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ClientDto>> UpdateClient(int id, [FromBody] UpdateClientDto updateClientDto)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
                return NotFound(new { message = $"Client with ID {id} not found" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            client.Name = updateClientDto.Name;
            client.ContactDetails = updateClientDto.ContactDetails;
            client.Region = updateClientDto.Region;

            await _context.SaveChangesAsync();

            var clientDto = new ClientDto
            {
                Id = client.Id,
                Name = client.Name,
                ContactDetails = client.ContactDetails,
                Region = client.Region
            };

            return Ok(clientDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
                return NotFound(new { message = $"Client with ID {id} not found" });

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
