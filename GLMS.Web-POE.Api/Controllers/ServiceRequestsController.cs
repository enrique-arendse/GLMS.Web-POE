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
    public class ServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> GetServiceRequests(
            [FromQuery] int? contractId = null,
            [FromQuery] int? status = null)
        {
            var query = _context.ServiceRequests.AsQueryable();

            if (contractId.HasValue)
                query = query.Where(sr => sr.ContractId == contractId.Value);

            if (status.HasValue)
                query = query.Where(sr => (int)sr.Status == status.Value);

            var serviceRequests = await query.ToListAsync();

            var dtos = serviceRequests.Select(sr => new ServiceRequestDto
            {
                Id = sr.Id,
                ContractId = sr.ContractId,
                Description = sr.Description,
                Cost = sr.Cost,
                Status = (int)sr.Status,
                AmountUsd = sr.AmountUsd,
                ExchangeRate = sr.ExchangeRate,
                CreatedAt = sr.CreatedAt
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequestDto>> GetServiceRequest(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);

            if (serviceRequest == null)
                return NotFound(new { message = $"Service request with ID {id} not found" });

            var dto = new ServiceRequestDto
            {
                Id = serviceRequest.Id,
                ContractId = serviceRequest.ContractId,
                Description = serviceRequest.Description,
                Cost = serviceRequest.Cost,
                Status = (int)serviceRequest.Status,
                AmountUsd = serviceRequest.AmountUsd,
                ExchangeRate = serviceRequest.ExchangeRate,
                CreatedAt = serviceRequest.CreatedAt
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceRequestDto>> CreateServiceRequest([FromBody] CreateServiceRequestDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var contractExists = await _context.Contracts.AnyAsync(c => c.Id == createDto.ContractId);
            if (!contractExists)
                return BadRequest(new { message = $"Contract with ID {createDto.ContractId} not found" });

            var serviceRequest = new ServiceRequest
            {
                ContractId = createDto.ContractId,
                Description = createDto.Description,
                Cost = createDto.Cost,
                Status = ServiceRequestStatus.Pending,
                AmountUsd = createDto.AmountUsd,
                ExchangeRate = createDto.ExchangeRate,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();

            var dto = new ServiceRequestDto
            {
                Id = serviceRequest.Id,
                ContractId = serviceRequest.ContractId,
                Description = serviceRequest.Description,
                Cost = serviceRequest.Cost,
                Status = (int)serviceRequest.Status,
                AmountUsd = serviceRequest.AmountUsd,
                ExchangeRate = serviceRequest.ExchangeRate,
                CreatedAt = serviceRequest.CreatedAt
            };

            return CreatedAtAction(nameof(GetServiceRequest), new { id = serviceRequest.Id }, dto);
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ServiceRequestDto>> UpdateServiceRequestStatus(
            int id,
            [FromBody] UpdateServiceRequestStatusDto updateStatusDto)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);

            if (serviceRequest == null)
                return NotFound(new { message = $"Service request with ID {id} not found" });

            if (!Enum.IsDefined(typeof(ServiceRequestStatus), updateStatusDto.Status))
                return BadRequest(new { message = "Invalid status value" });

            serviceRequest.Status = (ServiceRequestStatus)updateStatusDto.Status;
            await _context.SaveChangesAsync();

            var dto = new ServiceRequestDto
            {
                Id = serviceRequest.Id,
                ContractId = serviceRequest.ContractId,
                Description = serviceRequest.Description,
                Cost = serviceRequest.Cost,
                Status = (int)serviceRequest.Status,
                AmountUsd = serviceRequest.AmountUsd,
                ExchangeRate = serviceRequest.ExchangeRate,
                CreatedAt = serviceRequest.CreatedAt
            };

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceRequest(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);

            if (serviceRequest == null)
                return NotFound(new { message = $"Service request with ID {id} not found" });

            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
