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
    public class ContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContractsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContractDto>>> GetContracts(
            [FromQuery] int? status = null,
            [FromQuery] int? clientId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (status.HasValue)
                query = query.Where(c => (int)c.Status == status.Value);

            if (clientId.HasValue)
                query = query.Where(c => c.ClientId == clientId.Value);

            if (startDate.HasValue)
                query = query.Where(c => c.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.EndDate <= endDate.Value);

            var contracts = await query.ToListAsync();

            var contractDtos = contracts.Select(c => new ContractDto
            {
                Id = c.Id,
                ClientId = c.ClientId,
                ClientName = c.Client?.Name,
				ClientRegion = c.Client?.Region,
				ClientContactDetails = c.Client?.ContactDetails,
				StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = (int)c.Status,
                ServiceLevel = c.ServiceLevel,
                SignedAgreementFileName = c.SignedAgreementFileName,
                SignedAgreementFilePath = c.SignedAgreementFilePath
            }).ToList();

            return Ok(contractDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContractDto>> GetContract(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            var contractDto = new ContractDto
            {
                Id = contract.Id,
                ClientId = contract.ClientId,
                ClientName = contract.Client?.Name,
				ClientRegion = contract.Client?.Region,
				ClientContactDetails = contract.Client?.ContactDetails,
				StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = (int)contract.Status,
                ServiceLevel = contract.ServiceLevel,
                SignedAgreementFileName = contract.SignedAgreementFileName,
                SignedAgreementFilePath = contract.SignedAgreementFilePath
            };

            return Ok(contractDto);
        }

        [HttpPost]
        public async Task<ActionResult<ContractDto>> CreateContract([FromBody] CreateContractDto createContractDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var clientExists = await _context.Clients.AnyAsync(c => c.Id == createContractDto.ClientId);
            if (!clientExists)
                return BadRequest(new { message = $"Client with ID {createContractDto.ClientId} not found" });

            if (createContractDto.EndDate <= createContractDto.StartDate)
                return BadRequest(new { message = "End date must be after start date" });

            var contract = new Contract
            {
                ClientId = createContractDto.ClientId,
                StartDate = createContractDto.StartDate,
                EndDate = createContractDto.EndDate,
                Status = (ContractStatus)createContractDto.Status,
                ServiceLevel = createContractDto.ServiceLevel,
                SignedAgreementFileName = createContractDto.SignedAgreementFileName,
                SignedAgreementFilePath = createContractDto.SignedAgreementFilePath
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            var client = await _context.Clients.FindAsync(contract.ClientId);

            var contractDto = new ContractDto
            {
                Id = contract.Id,
                ClientId = contract.ClientId,
                ClientName = client?.Name,
				ClientRegion = client?.Region,
				ClientContactDetails = client?.ContactDetails,
				StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = (int)contract.Status,
                ServiceLevel = contract.ServiceLevel,
                SignedAgreementFileName = contract.SignedAgreementFileName,
                SignedAgreementFilePath = contract.SignedAgreementFilePath
            };

            return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, contractDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ContractDto>> UpdateContract(int id, [FromBody] UpdateContractDto updateContractDto)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (updateContractDto.EndDate <= updateContractDto.StartDate)
                return BadRequest(new { message = "End date must be after start date" });

            var clientExists = await _context.Clients.AnyAsync(c => c.Id == updateContractDto.ClientId);
            if (!clientExists)
                return BadRequest(new { message = $"Client with ID {updateContractDto.ClientId} not found" });

            contract.ClientId = updateContractDto.ClientId;
            contract.StartDate = updateContractDto.StartDate;
            contract.EndDate = updateContractDto.EndDate;
            contract.Status = (ContractStatus)updateContractDto.Status;
            contract.ServiceLevel = updateContractDto.ServiceLevel;

            if (!string.IsNullOrEmpty(updateContractDto.SignedAgreementFileName))
                contract.SignedAgreementFileName = updateContractDto.SignedAgreementFileName;

            if (!string.IsNullOrEmpty(updateContractDto.SignedAgreementFilePath))
                contract.SignedAgreementFilePath = updateContractDto.SignedAgreementFilePath;

            await _context.SaveChangesAsync();

            var contractDto = new ContractDto
            {
                Id = contract.Id,
                ClientId = contract.ClientId,
                ClientName = contract.Client?.Name,
				ClientRegion = contract.Client?.Region,
				ClientContactDetails = contract.Client?.ContactDetails,
				StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = (int)contract.Status,
                ServiceLevel = contract.ServiceLevel,
                SignedAgreementFileName = contract.SignedAgreementFileName,
                SignedAgreementFilePath = contract.SignedAgreementFilePath
            };

            return Ok(contractDto);
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ContractDto>> UpdateContractStatus(int id, [FromBody] UpdateContractStatusDto updateStatusDto)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            if (!Enum.IsDefined(typeof(ContractStatus), updateStatusDto.Status))
                return BadRequest(new { message = "Invalid status value" });

            contract.Status = (ContractStatus)updateStatusDto.Status;
            await _context.SaveChangesAsync();

            var contractDto = new ContractDto
            {
                Id = contract.Id,
                ClientId = contract.ClientId,
                ClientName = contract.Client?.Name,
                ClientRegion = contract.Client?.Region,
                ClientContactDetails = contract.Client?.ContactDetails,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = (int)contract.Status,
                ServiceLevel = contract.ServiceLevel,
                SignedAgreementFileName = contract.SignedAgreementFileName,
                SignedAgreementFilePath = contract.SignedAgreementFilePath
            };

            return Ok(contractDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
