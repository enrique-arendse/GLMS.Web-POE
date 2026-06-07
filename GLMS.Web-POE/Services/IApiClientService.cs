using GLMS.Web_POE.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace GLMS.Web_POE.Services
{
	public interface IApiClientService
	{
		Task<IEnumerable<Client>> GetClientsAsync();
		Task<Client?> GetClientAsync(int id);
		Task<Client> CreateClientAsync(Client client);
		Task<Client> UpdateClientAsync(int id, Client client);
		Task<bool> DeleteClientAsync(int id);
	}

	public class ApiClientService : IApiClientService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<ApiClientService> _logger;

		public ApiClientService(HttpClient httpClient, ILogger<ApiClientService> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		public async Task<IEnumerable<Client>> GetClientsAsync()
		{
			try
			{
				var response = await _httpClient.GetAsync("api/clients");
				if (!response.IsSuccessStatusCode)
					return new List<Client>();

				var clientDtos = await response.Content.ReadFromJsonAsync<List<ClientApiDto>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				return clientDtos?.Select(c => new Client
				{
					Id = c.Id,
					Name = c.Name,
					ContactDetails = c.ContactDetails,
					Region = c.Region
				}).ToList() ?? new List<Client>();
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error calling API: {ex.Message}");
				return new List<Client>();
			}
		}

		public async Task<Client?> GetClientAsync(int id)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/clients/{id}");
				if (!response.IsSuccessStatusCode)
					return null;

				var clientDto = await response.Content.ReadFromJsonAsync<ClientApiDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				if (clientDto == null)
					return null;

				return new Client
				{
					Id = clientDto.Id,
					Name = clientDto.Name,
					ContactDetails = clientDto.ContactDetails,
					Region = clientDto.Region
				};
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error calling API: {ex.Message}");
				return null;
			}
		}

		public async Task<Client> CreateClientAsync(Client client)
		{
			try
			{
				var createDto = new CreateClientApiDto
				{
					Name = client.Name,
					ContactDetails = client.ContactDetails,
					Region = client.Region
				};

				var response = await _httpClient.PostAsJsonAsync("api/clients", createDto);
				response.EnsureSuccessStatusCode();

				var clientDto = await response.Content.ReadFromJsonAsync<ClientApiDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				return new Client
				{
					Id = clientDto!.Id,
					Name = clientDto.Name,
					ContactDetails = clientDto.ContactDetails,
					Region = clientDto.Region
				};
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error calling API: {ex.Message}");
				throw;
			}
		}

		public async Task<Client> UpdateClientAsync(int id, Client client)
		{
			try
			{
				var updateDto = new UpdateClientApiDto
				{
					Name = client.Name,
					ContactDetails = client.ContactDetails,
					Region = client.Region
				};

				var response = await _httpClient.PutAsJsonAsync($"api/clients/{id}", updateDto);
				response.EnsureSuccessStatusCode();

				var clientDto = await response.Content.ReadFromJsonAsync<ClientApiDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				return new Client
				{
					Id = clientDto!.Id,
					Name = clientDto.Name,
					ContactDetails = clientDto.ContactDetails,
					Region = clientDto.Region
				};
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error calling API: {ex.Message}");
				throw;
			}
		}

		public async Task<bool> DeleteClientAsync(int id)
		{
			try
			{
				var response = await _httpClient.DeleteAsync($"api/clients/{id}");
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
	internal class ClientApiDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string ContactDetails { get; set; } = string.Empty;
		public string Region { get; set; } = string.Empty;
	}

	internal class CreateClientApiDto
	{
		public string Name { get; set; } = string.Empty;
		public string ContactDetails { get; set; } = string.Empty;
		public string Region { get; set; } = string.Empty;
	}

	internal class UpdateClientApiDto
	{
		public string Name { get; set; } = string.Empty;
		public string ContactDetails { get; set; } = string.Empty;
		public string Region { get; set; } = string.Empty;
	}
}
