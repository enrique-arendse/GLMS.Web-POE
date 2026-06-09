using GLMS.Web_POE.Models;
using GLMS.Web_POE.Services;
using System.Net.Http.Json;
using System.Text.Json;

public class ServiceRequestService : IServiceRequest
{
	private readonly HttpClient _http;
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	public ServiceRequestService(HttpClient http)
	{
		_http = http;
	}

	public async Task<List<ServiceRequest>> GetAllAsync()
	{
		var dtos = await _http.GetFromJsonAsync<List<ServiceRequestApiDto>>("api/servicerequests", JsonOptions)
			   ?? new List<ServiceRequestApiDto>();
		return dtos.Select(MapToModel).ToList();
	}

	public async Task<ServiceRequest?> GetByIdAsync(int id)
	{
		var dto = await _http.GetFromJsonAsync<ServiceRequestApiDto>($"api/servicerequests/{id}", JsonOptions);
		return dto == null ? null : MapToModel(dto);
	}

	public async Task<ServiceRequest> CreateAsync(int contractId, string description, decimal amountUsd, decimal exchangeRate, decimal cost)
	{
		var dto = new
		{
			contractId,
			description,
			amountUsd,
			exchangeRate,
			cost
		};

		var response = await _http.PostAsJsonAsync("api/servicerequests", dto);
		response.EnsureSuccessStatusCode();

		var created = await response.Content.ReadFromJsonAsync<ServiceRequestApiDto>(JsonOptions);
		return MapToModel(created!);
	}

	public async Task UpdateAsync(int id, ServiceRequest request)
	{
		var dto = new ServiceRequestApiDto
		{
			Id = request.Id,
			ContractId = request.ContractId,
			Description = request.Description,
			AmountUsd = request.AmountUsd,
			ExchangeRate = request.ExchangeRate,
			Cost = request.Cost,
			Status = (int)request.Status,
			CreatedAt = request.CreatedAt
		};

		var response = await _http.PutAsJsonAsync($"api/servicerequests/{id}", dto);
		response.EnsureSuccessStatusCode();
	}

	public async Task DeleteAsync(int id)
	{
		var response = await _http.DeleteAsync($"api/servicerequests/{id}");
		response.EnsureSuccessStatusCode();
	}

	private static ServiceRequest MapToModel(ServiceRequestApiDto dto) => new()
	{
		Id = dto.Id,
		ContractId = dto.ContractId,
		Contract = !string.IsNullOrEmpty(dto.ContractServiceLevel)
		? new Contract
		{
			Id = dto.ContractId,
			ServiceLevel = dto.ContractServiceLevel,
			Status = (ContractStatus)(dto.ContractStatus ?? 0) 
		}
		: null,
		Description = dto.Description,
		AmountUsd = dto.AmountUsd,
		ExchangeRate = dto.ExchangeRate,
		Cost = dto.Cost,
		Status = (ServiceRequestStatus)dto.Status,
		CreatedAt = dto.CreatedAt
	};

	private sealed class ServiceRequestApiDto
	{
		public int Id { get; set; }
		public int ContractId { get; set; }
		public string? ContractServiceLevel { get; set; }
		public int? ContractStatus { get; set; }
		public string Description { get; set; } = string.Empty;
		public decimal Cost { get; set; }
		public int Status { get; set; }
		public decimal? AmountUsd { get; set; }
		public decimal? ExchangeRate { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
