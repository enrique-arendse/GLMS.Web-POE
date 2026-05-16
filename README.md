# GLMS — Global License Management System

A web-based license and service request management platform built with ASP.NET Core MVC, Entity Framework Core, and SQL Server. The system allows businesses to manage clients, contracts, and service requests, with real-time USD to ZAR currency conversion and PDF agreement uploads.

---

## Tech Stack

- **Framework:** ASP.NET Core MVC (.NET 10)
- **ORM:** Entity Framework Core
- **Database:** SQL Server
- **Frontend:** Bootstrap 5
- **Testing:** xUnit
- **External API:** [ExchangeRate-API](https://www.exchangerate-api.com/) for live currency rates

---

## Features

### Client & Contract Management
Clients can be created and linked to contracts. Each contract tracks a service level, date range, status, and a signed PDF agreement that can be uploaded and downloaded directly from the system.

### Service Requests
Service requests are tied to contracts. When creating a request, the system validates that the linked contract is not Expired or On Hold. The USD amount entered is automatically converted to ZAR using a live exchange rate fetched from ExchangeRate-API, and both the rate and the converted cost are stored against the request.

### Search & Filtering
Contracts can be filtered by status and date range. Filters support multiple criteria simultaneously and are implemented with LINQ queries server-side.

### File Handling
PDF agreements are uploaded on contract creation and can be replaced on edit. Files are stored securely with GUID-based naming and validated by both extension and content type. Files can be downloaded at any time from the contract detail view.

---

## Project Structure

```
GLMS.Web-POE/
├── Controllers/
│   ├── ClientsController.cs
│   ├── ContractsController.cs
│   └── ServiceRequestsController.cs
├── Models/
│   ├── Client.cs
│   ├── Contract.cs
│   ├── ServiceRequest.cs
│   └── Enums/
├── Services/
│   ├── CurrencyService.cs
│   ├── FileService.cs
│   ├── ContractService.cs
│   └── ServiceRequestService.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Migrations/
└── Views/
    ├── Clients/
    ├── Contracts/
    └── ServiceRequests/

GLMS.Tests/
├── CurrencyServiceTests.cs
├── FileServiceTests.cs
├── ContractServiceTests.cs
└── ServiceRequestModelTests.cs
```

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server (local or remote)

### Setup

1. Clone the repository and navigate to the project folder.

2. Update the connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=GLMS;Trusted_Connection=True;"
   }
   ```

3. Apply database migrations:
   ```powershell
   dotnet ef database update
   ```

4. Run the application:
   ```powershell
   cd GLMS.Web-POE
   dotnet run
   ```

The application will be available at `https://localhost:7001`.

### Running Tests

```powershell
dotnet test GLMS.Tests
```

19 tests covering currency conversion, file validation, contract status logic, and model behaviour — all passing.

---

## Currency Conversion

When a service request is created, the app calls the ExchangeRate-API to fetch the current USD/ZAR rate. The entered USD amount is multiplied by this rate, rounded to 2 decimal places, and saved alongside the exchange rate used. This means each request has a historical record of the rate at the time it was created.

---

## Notes

- Service requests cannot be created against Expired or On Hold contracts.
- PDF uploads are restricted to `.pdf` files under 5MB.
- The application forces `en-US` culture to ensure decimal values (e.g. `18.50`) are handled correctly regardless of the server's regional settings.
