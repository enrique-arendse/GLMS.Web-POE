using GLMS.Web_POE.Services;
using GLMS.Web_POE.Models;
using GLMS.Web_POE.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace GLMS.Tests
{
	public class CurrencyServiceTests
	{
		[Fact]
		public void ConvertUsdToZar_WithValidRate_ReturnsCorrectAmount()
		{
			// Arrange
			var httpClient = new HttpClient();
			var currencyService = new CurrencyService(httpClient);
			decimal usdAmount = 100m;
			decimal exchangeRate = 18.50m;

			// Act
			var result = currencyService.ConvertUsdToZar(usdAmount, exchangeRate);

			// Assert
			Assert.Equal(1850m, result);
		}

		[Fact]
		public void ConvertUsdToZar_WithDecimalRate_RoundsCorrectly()
		{
			// Arrange
			var httpClient = new HttpClient();
			var currencyService = new CurrencyService(httpClient);
			decimal usdAmount = 100m;
			decimal exchangeRate = 18.456m;

			// Act
			var result = currencyService.ConvertUsdToZar(usdAmount, exchangeRate);

			// Assert
			Assert.Equal(1845.60m, result);
		}

		[Fact]
		public void ConvertUsdToZar_WithZeroAmount_ReturnsZero()
		{
			// Arrange
			var httpClient = new HttpClient();
			var currencyService = new CurrencyService(httpClient);
			decimal usdAmount = 0m;
			decimal exchangeRate = 18.50m;

			// Act
			var result = currencyService.ConvertUsdToZar(usdAmount, exchangeRate);

			// Assert
			Assert.Equal(0m, result);
		}

		[Fact]
		public void ConvertUsdToZar_WithLargeAmount_CalculatesCorrectly()
		{
			// Arrange
			var httpClient = new HttpClient();
			var currencyService = new CurrencyService(httpClient);
			decimal usdAmount = 10000m;
			decimal exchangeRate = 18.50m;

			// Act
			var result = currencyService.ConvertUsdToZar(usdAmount, exchangeRate);

			// Assert
			Assert.Equal(185000m, result);
		}
	}

	public class FileServiceTests
	{
		private IWebHostEnvironment GetMockEnvironment()
		{
			var mockEnvironment = new Mock<IWebHostEnvironment>();
			var tempPath = Path.Combine(Path.GetTempPath(), "wwwroot");
			mockEnvironment.Setup(e => e.WebRootPath).Returns(tempPath);
			return mockEnvironment.Object;
		}

		[Fact]
		public void IsPdf_WithPdfFile_ReturnsTrue()
		{
			// Arrange
			var fileService = new FileService(GetMockEnvironment());

			// Create a mock IFormFile with PDF content type
			var mockFile = new Mock<IFormFile>();
			mockFile.SetupGet(f => f.FileName).Returns("document.pdf");
			mockFile.SetupGet(f => f.ContentType).Returns("application/pdf");
			mockFile.SetupGet(f => f.Length).Returns(1000);

			// Act
			var result = fileService.IsPdf(mockFile.Object);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void IsPdf_WithNonPdfFile_ReturnsFalse()
		{
			// Arrange
			var fileService = new FileService(GetMockEnvironment());

			// Create a mock IFormFile with wrong extension
			var mockFile = new Mock<IFormFile>();
			mockFile.SetupGet(f => f.FileName).Returns("document.exe");
			mockFile.SetupGet(f => f.ContentType).Returns("application/octet-stream");
			mockFile.SetupGet(f => f.Length).Returns(1000);

			// Act
			var result = fileService.IsPdf(mockFile.Object);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void IsPdf_WithEmptyFile_ReturnsFalse()
		{
			// Arrange
			var fileService = new FileService(GetMockEnvironment());

			// Create a mock IFormFile that is empty
			var mockFile = new Mock<IFormFile>();
			mockFile.SetupGet(f => f.FileName).Returns("document.pdf");
			mockFile.SetupGet(f => f.ContentType).Returns("application/pdf");
			mockFile.SetupGet(f => f.Length).Returns(0);

			// Act
			var result = fileService.IsPdf(mockFile.Object);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void IsPdf_WithNullFile_ReturnsFalse()
		{
			// Arrange
			var fileService = new FileService(GetMockEnvironment());

			// Act
			var result = fileService.IsPdf(null);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task SaveAgreementAsync_WithValidPdf_ThrowsExceptionForNonPdf()
		{
			// Arrange
			var fileService = new FileService(GetMockEnvironment());

			var mockFile = new Mock<IFormFile>();
			mockFile.SetupGet(f => f.FileName).Returns("malicious.exe");
			mockFile.SetupGet(f => f.ContentType).Returns("application/octet-stream");
			mockFile.SetupGet(f => f.Length).Returns(1000);

			// Act & Assert
			await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			{
				await fileService.SaveAgreementAsync(mockFile.Object);
			});
		}
	}

	public class ContractServiceTests
	{
		[Fact]
		public void CanCreateServiceRequest_WithActiveContract_ReturnsTrue()
		{
			// Arrange
			var contractService = new ContractService();
			var contract = new Contract
			{
				Id = 1,
				Status = ContractStatus.Active
			};

			// Act
			var result = contractService.CanCreateServiceRequest(contract);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void CanCreateServiceRequest_WithDraftContract_ReturnsTrue()
		{
			// Arrange
			var contractService = new ContractService();
			var contract = new Contract
			{
				Id = 1,
				Status = ContractStatus.Draft
			};

			// Act
			var result = contractService.CanCreateServiceRequest(contract);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void CanCreateServiceRequest_WithExpiredContract_ReturnsFalse()
		{
			// Arrange
			var contractService = new ContractService();
			var contract = new Contract
			{
				Id = 1,
				Status = ContractStatus.Expired
			};

			// Act
			var result = contractService.CanCreateServiceRequest(contract);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void CanCreateServiceRequest_WithOnHoldContract_ReturnsFalse()
		{
			// Arrange
			var contractService = new ContractService();
			var contract = new Contract
			{
				Id = 1,
				Status = ContractStatus.OnHold
			};

			// Act
			var result = contractService.CanCreateServiceRequest(contract);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void CanCreateServiceRequest_WithNullContract_ReturnsFalse()
		{
			// Arrange
			var contractService = new ContractService();

			// Act
			var result = contractService.CanCreateServiceRequest(null);

			// Assert
			Assert.False(result);
		}
	}

	public class ServiceRequestModelTests
	{
		[Fact]
		public void ServiceRequest_CreatedAt_DefaultsToUtcNow()
		{
			// Arrange
			var serviceRequest = new ServiceRequest
			{
				Id = 1,
				ContractId = 1,
				Description = "Test Request",
				Cost = 1000m
			};

			// Act & Assert
			Assert.True((DateTime.UtcNow - serviceRequest.CreatedAt).TotalSeconds < 1);
		}

		[Fact]
		public void ServiceRequest_WithAllProperties_IsValid()
		{
			// Arrange
			var contract = new Contract { Id = 1 };
			var serviceRequest = new ServiceRequest
			{
				Id = 1,
				ContractId = 1,
				Contract = contract,
				Description = "Test Service Request",
				Cost = 1850m,
				Status = ServiceRequestStatus.Pending,
				AmountUsd = 100m,
				ExchangeRate = 18.50m,
				CreatedAt = DateTime.UtcNow
			};

			// Act & Assert
			Assert.NotNull(serviceRequest);
			Assert.Equal(1, serviceRequest.Id);
			Assert.Equal("Test Service Request", serviceRequest.Description);
			Assert.Equal(1850m, serviceRequest.Cost);
			Assert.Equal(ServiceRequestStatus.Pending, serviceRequest.Status);
		}
	}

	public class ContractModelTests
	{
		[Fact]
		public void Contract_WithClientAndDates_IsValid()
		{
			// Arrange
			var client = new Client { Id = 1, Name = "Test Client" };
			var startDate = DateTime.Now;
			var endDate = startDate.AddMonths(12);

			var contract = new Contract
			{
				Id = 1,
				ClientId = 1,
				Client = client,
				StartDate = startDate,
				EndDate = endDate,
				Status = ContractStatus.Active,
				ServiceLevel = "Premium"
			};

			// Act & Assert
			Assert.NotNull(contract);
			Assert.Equal("Premium", contract.ServiceLevel);
			Assert.Equal(ContractStatus.Active, contract.Status);
			Assert.NotNull(contract.Client);
		}
	}

	public class ContractFilteringTests
	{
		[Fact]
		public void GetContracts_FilterByStatus_ReturnsMatchingContracts()
		{
			// Arrange - Create in-memory database
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			using (var context = new ApplicationDbContext(options))
			{
				// Add test data
				var client = new Client { Id = 1, Name = "Test Client", ContactDetails = "test@example.com", Region = "SA" };
				context.Clients.Add(client);

				var contract1 = new Contract
				{
					Id = 1,
					ClientId = 1,
					StartDate = DateTime.Now,
					EndDate = DateTime.Now.AddMonths(12),
					Status = ContractStatus.Active,
					ServiceLevel = "Premium"
				};

				var contract2 = new Contract
				{
					Id = 2,
					ClientId = 1,
					StartDate = DateTime.Now,
					EndDate = DateTime.Now.AddMonths(12),
					Status = ContractStatus.Draft,
					ServiceLevel = "Standard"
				};

				context.Contracts.Add(contract1);
				context.Contracts.Add(contract2);
				context.SaveChanges();
			}

			using (var context = new ApplicationDbContext(options))
			{
				// Act
				var activeContracts = context.Contracts
					.Where(c => c.Status == ContractStatus.Active)
					.ToList();

				// Assert
				Assert.Single(activeContracts);
				Assert.Equal(ContractStatus.Active, activeContracts[0].Status);
			}
		}

		[Fact]
		public void GetContracts_FilterByDateRange_ReturnsMatchingContracts()
		{
			// Arrange - Create in-memory database
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			using (var context = new ApplicationDbContext(options))
			{
				var client = new Client { Id = 1, Name = "Test Client", ContactDetails = "test@example.com", Region = "SA" };
				context.Clients.Add(client);

				var startDate1 = new DateTime(2024, 1, 1);
				var contract1 = new Contract
				{
					Id = 1,
					ClientId = 1,
					StartDate = startDate1,
					EndDate = startDate1.AddMonths(12),
					Status = ContractStatus.Active,
					ServiceLevel = "Premium"
				};

				var startDate2 = new DateTime(2025, 6, 1);
				var contract2 = new Contract
				{
					Id = 2,
					ClientId = 1,
					StartDate = startDate2,
					EndDate = startDate2.AddMonths(12),
					Status = ContractStatus.Draft,
					ServiceLevel = "Standard"
				};

				context.Contracts.Add(contract1);
				context.Contracts.Add(contract2);
				context.SaveChanges();
			}

			using (var context = new ApplicationDbContext(options))
			{
				// Act
				var targetDate = new DateTime(2024, 6, 1);
				var contractsInRange = context.Contracts
					.Where(c => c.StartDate <= targetDate && c.EndDate >= targetDate)
					.ToList();

				// Assert
				Assert.Single(contractsInRange);
				Assert.Equal(1, contractsInRange[0].Id);
			}
		}
	}
}
