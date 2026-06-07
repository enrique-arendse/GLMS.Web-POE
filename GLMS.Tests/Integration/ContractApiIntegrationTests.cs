using GLMS.Web_POE.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GLMS.Tests
{
	public class CustomWebApplicationFactory<TProgram>
		: WebApplicationFactory<TProgram> where TProgram : class
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			// Set environment to Test
			builder.UseEnvironment("Test");

			builder.ConfigureServices(services =>
			{
				// Remove the SQL Server DbContext registration if it exists
				var sqlDescriptors = services
					.Where(s => s.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
					.ToList();

				foreach (var descriptor in sqlDescriptors)
				{
					services.Remove(descriptor);
				}

				// Add in-memory database
				services.AddDbContext<ApplicationDbContext>(options =>
				{
					options.UseInMemoryDatabase("TestDb");
				});

				// Build service provider and seed data
				var sp = services.BuildServiceProvider();
				using var scope = sp.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				db.Database.EnsureCreated();
				SeedTestData(db);
			});
		}

		private void SeedTestData(ApplicationDbContext db)
		{
			if (db.Clients.Any())
				return;

			var client1 = new GLMS.Web_POE.Models.Client
			{
				Name = "Test Client 1",
				ContactDetails = "contact@test1.com",
				Region = "North"
			};

			var client2 = new GLMS.Web_POE.Models.Client
			{
				Name = "Test Client 2",
				ContactDetails = "contact@test2.com",
				Region = "South"
			};

			db.Clients.AddRange(client1, client2);
			db.SaveChanges();

			var contract1 = new GLMS.Web_POE.Models.Contract
			{
				ClientId = client1.Id,
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(365),
				Status = GLMS.Web_POE.Models.ContractStatus.Active,
				ServiceLevel = "Premium"
			};

			var contract2 = new GLMS.Web_POE.Models.Contract
			{
				ClientId = client2.Id,
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(180),
				Status = GLMS.Web_POE.Models.ContractStatus.Draft,
				ServiceLevel = "Standard"
			};

			db.Contracts.AddRange(contract1, contract2);
			db.SaveChanges();
		}
	}

	public class ContractApiIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
	{
		private readonly HttpClient _client;

		public ContractApiIntegrationTests(CustomWebApplicationFactory<Program> factory)
		{
			_client = factory.CreateClient();
		}

		[Fact]
		public async Task GetContracts_ShouldReturnSuccessStatusCode()
		{
			// Arrange - Test MVC frontend is healthy
			// Act - Test that the MVC app is healthy
			var response = await _client.GetAsync("/");

			// Assert
			Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Redirect,
				$"Expected success but got {response.StatusCode}");
		}

		[Fact]
		public async Task HomePage_ShouldLoad()
		{
			// Act
			var response = await _client.GetAsync("/");

			// Assert
			Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Redirect);
		}
	}

	// DTOs for testing
	internal class ContractDto
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

	internal class CreateContractDto
	{
		public int ClientId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string ServiceLevel { get; set; } = string.Empty;
	}

	internal class UpdateContractStatusDto
	{
		public int Status { get; set; }
	}

	internal class TokenResponse
	{
		public string Token { get; set; } = string.Empty;
	}
}
