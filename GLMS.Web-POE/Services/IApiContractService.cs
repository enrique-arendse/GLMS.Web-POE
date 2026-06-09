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
		Task<Contract> UpdateContractAsync(int id, Contract contract);
		Task<Contract> UpdateContractStatusAsync(int id, int status);
		Task<bool> DeleteContractAsync(int id);
	}

	public class ApiContractService : IApiContractService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<ApiContractService> _logger;

		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNameCaseInsensitive = true
		};

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

				var contracts = await response.Content.ReadFromJsonAsync<List<ContractApiDto>>(JsonOptions);
				return contracts?.Select(MapToContract).ToList() ?? new List<Contract>();
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

				var contractDto = await response.Content.ReadFromJsonAsync<ContractApiDto>(JsonOptions);
				return contractDto == null ? null : MapToContract(contractDto);
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
					ServiceLevel = contract.ServiceLevel,
					SignedAgreementFileName = contract.SignedAgreementFileName,
					SignedAgreementFilePath = contract.SignedAgreementFilePath
				};

				var response = await _httpClient.PostAsJsonAsync("api/contracts", createDto);
				response.EnsureSuccessStatusCode();

				var contractDto = await response.Content.ReadFromJsonAsync<ContractApiDto>(JsonOptions);
				return MapToContract(contractDto!);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error calling API: {ex.Message}");
				throw;
			}
		}

		public async Task<Contract> UpdateContractAsync(int id, Contract contract)
		{
			try
			{
				var updateDto = new UpdateContractApiDto
				{
					ClientId = contract.ClientId,
					StartDate = contract.StartDate,
					EndDate = contract.EndDate,
					Status = (int)contract.Status,
					ServiceLevel = contract.ServiceLevel,
					SignedAgreementFileName = contract.SignedAgreementFileName,
					SignedAgreementFilePath = contract.SignedAgreementFilePath
				};

				var response = await _httpClient.PutAsJsonAsync($"api/contracts/{id}", updateDto);
				response.EnsureSuccessStatusCode();

				var contractDto = await response.Content.ReadFromJsonAsync<ContractApiDto>(JsonOptions);
				return MapToContract(contractDto!);
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

				var contractDto = await response.Content.ReadFromJsonAsync<ContractApiDto>(JsonOptions);
				return MapToContract(contractDto!);
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

		private static Contract MapToContract(ContractApiDto dto) => new()
		{
			Id = dto.Id,
			ClientId = dto.ClientId,
			Client = !string.IsNullOrEmpty(dto.ClientName)
				? new Client
				{
				Id = dto.ClientId,
				Name = dto.ClientName,
				Region = dto.ClientRegion,                
				ContactDetails = dto.ClientContactDetails 
				}
				: null,
			StartDate = dto.StartDate,
			EndDate = dto.EndDate,
			Status = (ContractStatus)dto.Status,
			ServiceLevel = dto.ServiceLevel,
			SignedAgreementFileName = dto.SignedAgreementFileName,
			SignedAgreementFilePath = dto.SignedAgreementFilePath
		};
	}

	internal class ContractApiDto
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public string? ClientName { get; set; }
		public string? ClientRegion { get; set; }           
		public string? ClientContactDetails { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string ServiceLevel { get; set; } = string.Empty;
		public string? SignedAgreementFileName { get; set; }
		public string? SignedAgreementFilePath { get; set; }
	}

	internal class CreateContractApiDto
	{
		public int ClientId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string ServiceLevel { get; set; } = string.Empty;
		public string? SignedAgreementFileName { get; set; }
		public string? SignedAgreementFilePath { get; set; }
	}

	internal class UpdateContractApiDto
	{
		public int ClientId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string ServiceLevel { get; set; } = string.Empty;
		public string? SignedAgreementFileName { get; set; }
		public string? SignedAgreementFilePath { get; set; }
	}

	internal class UpdateContractStatusApiDto
	{
		public int Status { get; set; }
	}
}
