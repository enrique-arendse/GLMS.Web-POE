using GLMS.Web_POE.Data;
using GLMS.Web_POE.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GLMS.Tests.Integration
{
	public class ApiWebApplicationFactory : WebApplicationFactory<GLMS.Web_POE.Api.Program>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.UseEnvironment("Test");

		}

		internal static void SeedTestData(ApplicationDbContext db)
		{
			if (db.Clients.Any())
				return;

			var client = new Client
			{
				Name = "Test Client",
				ContactDetails = "contact@test.com",
				Region = "Gauteng"
			};

			db.Clients.Add(client);
			db.SaveChanges();

			db.Contracts.Add(new Contract
			{
				ClientId = client.Id,
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(365),
				Status = ContractStatus.Active,
				ServiceLevel = "Premium"
			});

			db.SaveChanges();
		}
	}

	public class ContractApiIntegrationTests : IClassFixture<ApiWebApplicationFactory>
	{
		private readonly HttpClient _client;
		private readonly JsonSerializerOptions _jsonOptions = new()
		{
			PropertyNameCaseInsensitive = true
		};

		public ContractApiIntegrationTests(ApiWebApplicationFactory factory)
		{
			using (var scope = factory.Services.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				db.Database.EnsureCreated();
				ApiWebApplicationFactory.SeedTestData(db);
			}

			_client = factory.CreateClient();
		}

		private async Task AuthenticateAsync()
		{
			var tokenResponse = await _client.PostAsync("/api/auth/token?username=admin", null);
			tokenResponse.EnsureSuccessStatusCode();

			var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>(_jsonOptions);
			Assert.NotNull(token);
			Assert.False(string.IsNullOrWhiteSpace(token.Token));

			_client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Bearer", token.Token);
		}

		[Fact]
		public async Task GetContracts_ShouldReturnSuccessStatusCode()
		{
			await AuthenticateAsync();

			var response = await _client.GetAsync("/api/contracts");

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			var contracts = await response.Content.ReadFromJsonAsync<List<ContractDto>>(_jsonOptions);
			Assert.NotNull(contracts);
			Assert.NotEmpty(contracts);
		}

		[Fact]
		public async Task CreateClient_ThenGetClient_ShouldPersistData()
		{
			await AuthenticateAsync();

			var createDto = new CreateClientDto
			{
				Name = "Integration Client",
				ContactDetails = "integration@example.com",
				Region = "Western Cape"
			};

			var createResponse = await _client.PostAsJsonAsync("/api/clients", createDto);
			Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

			var createdClient = await createResponse.Content.ReadFromJsonAsync<ClientDto>(_jsonOptions);
			Assert.NotNull(createdClient);
			Assert.True(createdClient.Id > 0);
			Assert.Equal(createDto.Name, createdClient.Name);

			var getResponse = await _client.GetAsync($"/api/clients/{createdClient.Id}");
			Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

			var fetchedClient = await getResponse.Content.ReadFromJsonAsync<ClientDto>(_jsonOptions);
			Assert.NotNull(fetchedClient);
			Assert.Equal(createdClient.Id, fetchedClient.Id);
			Assert.Equal(createDto.ContactDetails, fetchedClient.ContactDetails);
		}

		[Fact]
		public async Task GetContracts_WithoutToken_ShouldReturnUnauthorized()
		{
			var response = await _client.GetAsync("/api/contracts");
			Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
		}
	}

	internal class ContractDto
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public string? ClientName { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string ServiceLevel { get; set; } = string.Empty;
	}

	internal class ClientDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string ContactDetails { get; set; } = string.Empty;
		public string Region { get; set; } = string.Empty;
	}

	internal class CreateClientDto
	{
		public string Name { get; set; } = string.Empty;
		public string ContactDetails { get; set; } = string.Empty;
		public string Region { get; set; } = string.Empty;
	}

	internal class TokenResponse
	{
		public string Token { get; set; } = string.Empty;
	}
}
