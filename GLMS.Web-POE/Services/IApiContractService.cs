using GLMS.Web_POE.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace GLMS.Web_POE.Services
{
	public interface IApiContractService
	{
		Task<IEnumerable<Contract>> GetContractsAsync(int? status = null, int? clientId = null, DateTime? startDate = null, DateTime? endDate = null);
		Task<Contract?> GetContractAsync(int id);
		Task<Contract> CreateContractAsync(Contract contract);
		Task<Contract> UpdateContractStatusAsync(int id, int status);
		Task<bool> DeleteContractAsync(int id);
	}

	public class ApiContractService : IApiContractService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<ApiContractService> _logger;

		public ApiContractService(HttpClient httpClient, ILogger<ApiContractService> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		public async Task<IEnumerable<Contract>> GetContractsAsync(int? status = null, int? clientId = null, DateTime? startDate = null, DateTime? endDate = null)
		{
			try
			{
				var queryParams = new List<string>();
				if (status.HasValue) queryParams.Add($"status={status}");
				if (clientId.HasValue) queryParams.Add($"clientId={clientId}");
				if (startDate.HasValue) queryParams.Add($"startDate={startDate:yyyy-MM-dd}");
				if (endDate.HasValue) queryParams.Add($"endDate={endDate:yyyy-MM-dd}");

				var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
				var response = await _httpClient.GetAsync($"api/contracts{query}");

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError($"API call failed: {response.StatusCode}");
					return new List<Contract>();
				}

				var contracts = await response.Content.ReadFromJsonAsync<List<ContractApiDto>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				return contracts?.Select(c => new Contract
				{
					Id = c.Id,
					ClientId = c.ClientId,
					StartDate = c.StartDate,
					EndDate = c.EndDate,
					Status = (ContractStatus)c.Status,
					ServiceLevel = c.ServiceLevel,
					SignedAgreementFileName = c.SignedAgreementFileName
				}).ToList() ?? new List<Contract>();
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error calling API: {ex.Message}");
				return new List<Contract>();
			}
		}

		public async Task<Contract?> GetContractAsync(int id)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/contracts/{id}");
				if (!response.IsSuccessStatusCode)
					return null;

				var contractDto = await response.Content.ReadFromJsonAsync<ContractApiDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				if (contractDto == null)
					return null;

				return new Contract
				{
					Id = contractDto.Id,
					ClientId = contractDto.ClientId,
					StartDate = contractDto.StartDate,
					EndDate = contractDto.EndDate,
					Status = (ContractStatus)contractDto.Status,
					ServiceLevel = contractDto.ServiceLevel,
					SignedAgreementFileName = contractDto.SignedAgreementFileName
				};
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error calling API: {ex.Message}");
				return null;
			}
		}

		public async Task<Contract> CreateContractAsync(Contract contract)
		{
			try
			{
				var createDto = new CreateContractApiDto
				{
					ClientId = contract.ClientId,
					StartDate = contract.StartDate,
					EndDate = contract.EndDate,
					Status = (int)contract.Status,
					ServiceLevel = contract.ServiceLevel
				};

				var response = await _httpClient.PostAsJsonAsync("api/contracts", createDto);
				response.EnsureSuccessStatusCode();

				var contractDto = await response.Content.ReadFromJsonAsync<ContractApiDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				return new Contract
				{
					Id = contractDto!.Id,
					ClientId = contractDto.ClientId,
					StartDate = contractDto.StartDate,
					EndDate = contractDto.EndDate,
					Status = (ContractStatus)contractDto.Status,
					ServiceLevel = contractDto.ServiceLevel,
					SignedAgreementFileName = contractDto.SignedAgreementFileName
				};
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error calling API: {ex.Message}");
				throw;
			}
		}

		public async Task<Contract> UpdateContractStatusAsync(int id, int status)
		{
			try
			{
				var updateDto = new UpdateContractStatusApiDto { Status = status };
				var response = await _httpClient.PatchAsJsonAsync($"api/contracts/{id}/status", updateDto);
				response.EnsureSuccessStatusCode();

				var contractDto = await response.Content.ReadFromJsonAsync<ContractApiDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				return new Contract
				{
					Id = contractDto!.Id,
					ClientId = contractDto.ClientId,
					StartDate = contractDto.StartDate,
					EndDate = contractDto.EndDate,
					Status = (ContractStatus)contractDto.Status,
					ServiceLevel = contractDto.ServiceLevel,
					SignedAgreementFileName = contractDto.SignedAgreementFileName
				};
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error calling API: {ex.Message}");
				throw;
			}
		}

		public async Task<bool> DeleteContractAsync(int id)
		{
			try
			{
				var response = await _httpClient.DeleteAsync($"api/contracts/{id}");
				return response.IsSuccessStatusCode;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error calling API: {ex.Message}");
				return false;
			}
		}
	}

	// DTOs for API communication
	internal class ContractApiDto
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public string? ClientName { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string ServiceLevel { get; set; } = string.Empty;
		public string? SignedAgreementFileName { get; set; }
	}

	internal class CreateContractApiDto
	{
		public int ClientId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string ServiceLevel { get; set; } = string.Empty;
	}

	internal class UpdateContractStatusApiDto
	{
		public int Status { get; set; }
	}
}
