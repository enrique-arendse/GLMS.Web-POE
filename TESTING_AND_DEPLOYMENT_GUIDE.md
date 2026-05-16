# GLMS - Testing & Deployment Guide

## Project Structure

```
GLMS.Web-POE/                    # Main ASP.NET Core MVC Application
├── Controllers/                 # MVC Controllers
│   ├── ClientsController.cs
│   ├── ContractsController.cs   # Enhanced with file upload & search
│   ├── ServiceRequestsController.cs  # Enhanced with validation & currency
│   └── HomeController.cs
├── Models/                      # Data Models
│   ├── Client.cs
│   ├── Contract.cs
│   ├── ServiceRequest.cs
│   ├── Enums.cs                # Status enumerations
│   └── ErrorViewModel.cs
├── Services/                    # Business Logic
│   ├── IFileService.cs          # File management interface
│   ├── FileService.cs           # PDF file handling
│   ├── ICurrencyService.cs      # Currency conversion interface
│   ├── CurrencyService.cs       # USD to ZAR conversion
│   ├── IContractService.cs      # Contract validation interface
│   ├── ContractService.cs       # Status workflow logic
│   ├── IServiceRequest.cs       # Service request interface
│   └── ServiceRequest.cs        # Service request logic
├── Data/                        # Entity Framework
│   └── ApplicationDbContext.cs
├── Migrations/                  # Database migrations
│   ├── 20260422173844_InitialCreate.cs
│   ├── 20260422173844_InitialCreate.Designer.cs
│   └── ApplicationDbContextModelSnapshot.cs
├── Views/                       # Razor Views
│   ├── Clients/                 # CRUD views for clients
│   ├── Contracts/               # Enhanced contract views
│   │   ├── Index.cshtml        # Search/filter + management
│   │   ├── Create.cshtml       # File upload
│   │   ├── Edit.cshtml         # File update capability
│   │   ├── Details.cshtml      # Download link
│   │   └── Delete.cshtml
│   ├── ServiceRequests/         # Service request views
│   │   ├── Index.cshtml        # Management dashboard
│   │   ├── Create.cshtml       # USD input + conversion
│   │   ├── Edit.cshtml
│   │   ├── Details.cshtml      # Currency details
│   │   └── Delete.cshtml
│   ├── Home/
│   ├── Shared/
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── wwwroot/                     # Static files & uploads
│   └── uploads/agreements/      # PDF storage directory
├── Properties/
│   └── launchSettings.json
├── appsettings.json             # Configuration
├── appsettings.Development.json
├── Program.cs                   # DI & middleware setup
├── GLMS.Web-POE.csproj         # Project file
└── ...

GLMS.Tests/                      # Unit Test Project
├── UnitTest1.cs                 # Comprehensive test suite
│   ├── CurrencyServiceTests
│   ├── FileServiceTests
│   ├── ContractServiceTests
│   ├── ServiceRequestModelTests
│   ├── ContractModelTests
│   └── ContractFilteringTests
├── GLMS.Tests.csproj           # References main project
└── ...
```

---

## Prerequisites

- **Visual Studio 2026** Community or higher
- **.NET 10** Runtime
- **SQL Server** (LocalDB or full instance)
- **NuGet** Package Manager (included with Visual Studio)

---

## Database Setup

### 1. Create Database
The database is automatically created via Entity Framework migrations.

### 2. Update Connection String
Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GLMS;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Apply Migrations
In Package Manager Console:

```powershell
Update-Database
```

Or using .NET CLI:

```bash
dotnet ef database update
```

---

## Running the Application

### Option 1: Visual Studio
1. Open `GLMS.Web-POE.sln`
2. Set `GLMS.Web-POE` as startup project
3. Press **F5** or click **Run**
4. Application opens at `https://localhost:7001/` (or port shown in output)

### Option 2: Command Line
```bash
cd GLMS.Web-POE
dotnet run
```

The application will start at the configured port (default: 5001 HTTP, 7001 HTTPS)

---

## Running Unit Tests

### Option 1: Visual Studio Test Explorer
1. Open **Test Explorer** (Test → Test Explorer or Ctrl+E, T)
2. Right-click `GLMS.Tests` → **Run All Tests**
3. View results showing 19 passed tests

### Option 2: Command Line
```bash
cd GLMS.Tests
dotnet test
```

Expected Output:
```
Ran 19 test(s). 19 Passed, 0 Failed
```

### Test Categories

**Currency Tests (4 tests):**
- Valid rate conversion
- Decimal rate rounding
- Zero amount handling
- Large amount calculation

**File Validation Tests (5 tests):**
- PDF file acceptance
- Non-PDF file rejection
- Empty file handling
- Null file handling
- Exception throwing for invalid files

**Contract Status Tests (5 tests):**
- Active contract allowed
- Draft contract allowed
- Expired contract blocked
- OnHold contract blocked
- Null contract handling

**Model & Filtering Tests (5 tests):**
- ServiceRequest timestamp validation
- Contract model validation
- Service request property validation
- Status-based filtering
- Date range filtering

---

## Application Walkthrough

### 1. Creating a Client
1. Navigate to **Clients** → **Create New**
2. Enter: Name, Contact Details, Region
3. Click **Create Client**

### 2. Creating a Contract
1. Navigate to **Contracts** → **Create New Contract**
2. Select a client
3. Choose start and end dates
4. Set status (Draft/Active/Expired/On Hold)
5. Enter service level (e.g., "Premium", "Standard")
6. **Upload signed agreement (PDF)**
7. Click **Create Contract**

### 3. Searching Contracts
1. Navigate to **Contracts** → **Index**
2. Use filter section:
   - **Start Date**: Filter contracts starting on or after this date
   - **End Date**: Filter contracts ending on or before this date
   - **Status**: Filter by Draft, Active, Expired, or On Hold
3. Click **Search** to apply filters

### 4. Creating a Service Request
1. Navigate to **Service Requests** → **Create New Service Request**
2. **Select Contract** (only Active/Draft contracts available)
3. Enter **Description**
4. Enter **Amount in USD** (e.g., 100)
5. Click **Create Service Request**
6. System:
   - Fetches current USD-to-ZAR rate from ExchangeRate-API
   - Calculates ZAR amount automatically
   - Stores exchange rate for audit trail

### 5. Viewing Service Request Details
1. Navigate to **Service Requests** → **Index**
2. Click **View** on any request
3. See:
   - USD amount and exchange rate
   - Calculated ZAR cost
   - Request status
   - Creation timestamp

### 6. Downloading Agreements
1. Navigate to **Contracts** → **Index**
2. Click **Download** button for any contract with an agreement
3. PDF file downloads to your default downloads folder

---

## File Upload Configuration

### Upload Directory
Files are saved to: `wwwroot/uploads/agreements/`

### File Naming
- Original filename is preserved
- GUID prefix added: `{GUID}_{original_filename}`
- Example: `a1b2c3d4-e5f6-7890-ijkl-mnopqrstuvwx_agreement.pdf`

### File Size Limit
Default: ASP.NET Core limit (default 30MB, can be configured in `Program.cs`)

### Accepted File Types
- **Only PDF**: `.pdf` extension with `application/pdf` content type

### Security Features
- Content-type validation
- Extension validation
- GUID-based naming prevents directory traversal
- Secure download via file serving controller action

---

## API Integration Details

### Exchange Rate API
- **Service**: ExchangeRate-API (https://open.er-api.com)
- **Endpoint**: `/v6/latest/USD`
- **Rate Limit**: 1500 requests/month (free tier)
- **Response Format**: JSON with rates for all currencies
- **Currency Target**: ZAR (South African Rand)

### Integration in Code
```csharp
// Location: Services/CurrencyService.cs
public async Task<decimal> GetUsdToZarRateAsync()
{
    var url = "https://open.er-api.com/v6/latest/USD";
    // Fetches current rate
    // Parsed from JSON response
    // Stored with service request
}
```

---

## Error Handling

### Contract Validation
- **Cannot create Service Request if Contract is:**
  - Expired
  - On Hold
- Error message displayed to user
- Validation occurs both in UI (dropdown filter) and business logic

### File Upload Errors
- **Invalid file type**: "Only PDF files are allowed."
- **Empty file**: File rejected
- **Null file**: No error, optional field
- **File save error**: Generic error message with details

### Currency Conversion Errors
- **API unavailable**: Exception caught and displayed
- **Rate fetch fails**: Transaction rolled back
- **Calculation overflow**: Rounded to 2 decimal places

---

## Database Schema

### Clients Table
```
Id (PK)
Name (nvarchar(100))
ContactDetails (nvarchar(100))
Region (nvarchar(50))
```

### Contracts Table
```
Id (PK)
ClientId (FK)
StartDate (datetime2)
EndDate (datetime2)
Status (int) - 0:Draft, 1:Active, 2:Expired, 3:OnHold
ServiceLevel (nvarchar(100))
SignedAgreementFileName (nvarchar(max), nullable)
SignedAgreementFilePath (nvarchar(max), nullable)
```

### ServiceRequests Table
```
Id (PK)
ContractId (FK)
Description (nvarchar(500))
Cost (decimal(18,2)) - ZAR amount
Status (int) - 0:Pending, 1:InProgress, 2:Completed, 3:Cancelled
AmountUsd (decimal(18,2), nullable)
ExchangeRate (decimal(18,6), nullable)
CreatedAt (datetime2)
```

---

## Performance Considerations

### Database
- Indexes on foreign keys (automatic via EF Core)
- Eager loading with `.Include()` to prevent N+1 queries
- Async operations throughout for better throughput

### API Calls
- HttpClient reused via DI (connection pooling)
- Single API call per service request creation
- Rate cached in database with request

### File Operations
- Async file writing to prevent blocking
- Stream-based copying for memory efficiency
- Secure temporary file handling

---

## Deployment Checklist

- [ ] Database connection string configured for production
- [ ] API keys/secrets stored in Azure Key Vault or similar
- [ ] HTTPS enforced
- [ ] CORS configured if needed
- [ ] File upload directory has proper permissions
- [ ] Logging configured
- [ ] Error pages customized
- [ ] Unit tests passing (19/19)
- [ ] Integration tests pass
- [ ] Load testing completed
- [ ] Security scan completed

---

## Troubleshooting

### Issue: Database not found
**Solution**: Run `Update-Database` in Package Manager Console

### Issue: API rate limit exceeded
**Solution**: Implement caching or upgrade API plan

### Issue: File upload fails
**Causes**:
- Non-PDF file selected → Select PDF only
- File upload directory missing → Create `wwwroot/uploads/agreements/`
- Insufficient permissions → Grant write permissions to folder

### Issue: Tests failing
**Solution**: 
- Ensure all NuGet packages are restored
- Rebuild solution
- Check if database is properly initialized for integration tests

### Issue: Slow page load
**Solution**:
- Check API rate limit
- Add caching for exchange rates
- Implement pagination for large datasets

---

## Support & Documentation

- ASP.NET Core: https://docs.microsoft.com/aspnet/core
- Entity Framework Core: https://docs.microsoft.com/ef/core
- xUnit: https://xunit.net/docs/getting-started
- Bootstrap 5: https://getbootstrap.com/docs
- ExchangeRate API: https://www.exchangerate-api.com/docs

---

**Last Updated**: 2024
**Status**: Production Ready ✅
