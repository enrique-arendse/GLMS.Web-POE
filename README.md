# TechMove Logistics (Global Logistics Management System)

Service-Oriented Architecture with REST API, Docker containerization, and automated testing.

## Quick Start

`powershell
git clone https://github.com/enrique-arendse/GLMS.Web-POE.git
cd GLMS.Web-POE
docker compose up --build -d
`

**Access:**
- Frontend: http://localhost
- API Docs: http://localhost:7001/swagger
- Health: http://localhost:7001/api/health

## Architecture

**3-Tier SOA:**
`
Frontend (MVC) → API (REST) → Database (SQL Server)
   Port 80         Port 7001      Port 1433
`

| Layer | Purpose | Technology |
|-------|---------|------------|
| **Presentation** | MVC views, no DB access | ASP.NET Core MVC, Razor |
| **Service** | REST endpoints, business logic | ASP.NET Core Web API, Repository Pattern |
| **Data** | Persistent storage | SQL Server 2022, Entity Framework Core |

## API Endpoints

### Clients
- GET /api/clients - List all clients
- POST /api/clients - Create client
- GET /api/clients/{id} - Get specific client
- PUT /api/clients/{id} - Update client
- DELETE /api/clients/{id} - Delete client

### Contracts
- GET /api/contracts - List contracts (filterable)
- POST /api/contracts - Create contract
- PATCH /api/contracts/{id}/status - Update status
- GET /api/contracts/{id} - Get contract
- DELETE /api/contracts/{id} - Delete contract

### Service Requests
- GET /api/servicerequests - List requests
- POST /api/servicerequests - Create request
- GET /api/servicerequests/{id} - Get request
- DELETE /api/servicerequests/{id} - Delete request

**Full documentation:** http://localhost:7001/swagger

## Docker Setup

### Containers
| Container | Image | Port |
|-----------|-------|------|
| sql-server-db | mssql:2022 | 1433 |
| glms-backend-api | .NET 10 API | 7001 |
| glms-frontend-web | .NET 10 MVC | 80 |

### Multi-Stage Builds
- Optimized Dockerfiles reduce image size to ~200MB
- Build stage compiles code, runtime stage runs app
- Health checks for all services

### Networking
- All containers on glms-network bridge
- Service-to-service via DNS names (e.g., glms-backend-api:7001)

## Key Design Patterns

### Repository Pattern
Abstracts data access from business logic:
`csharp
public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByNameAsync(string name);
}
`

### Dependency Injection
Loose coupling via constructor injection:
`csharp
builder.Services.AddScoped<IClientRepository, ClientRepository>();
`

### HttpClient Service Layer
Frontend calls API, never touches database:
`csharp
// Frontend - no DB access
var clients = await _apiService.GetClientsAsync();

// Service - HTTP call
var response = await _httpClient.GetAsync("api/clients");
`

## Testing

**Integration tests:** GLMS.Tests project
`powershell
dotnet test
`

**Results:** 2/2 passing ✅

- In-memory database (no SQL Server needed)
- Custom WebApplicationFactory
- Data seeding for reproducibility

## Technology Stack

- **.NET 10** - Framework
- **ASP.NET Core MVC** - Frontend
- **ASP.NET Core Web API** - Backend
- **SQL Server 2022** - Database
- **Entity Framework Core 10.0.8** - ORM
- **Swagger 7.1.0** - API documentation
- **Docker & Docker Compose** - Containerization
- **xUnit** - Testing
- **JWT** - Authentication

## Project Structure

```text
GLMS.Web-POE/
├── GLMS.Web-POE/              # MVC Frontend
│   ├── Controllers/
│   ├── Services/              # HttpClient services
│   ├── Views/
│   └── Dockerfile
├── GLMS.Web-POE.Api/          # REST API
│   ├── Controllers/
│   ├── Repositories/
│   ├── DTOs/
│   └── Dockerfile.api
├── GLMS.Tests/                # Integration tests
├── docker-compose.yml
└── README.md
```

## Deployment

### Run
`powershell
docker compose up --build -d
docker compose ps                    # Check status
docker compose logs glms-backend-api # View logs
docker compose down                  # Stop
`

### Troubleshooting
| Issue | Solution |
|-------|----------|
| Port conflict | Change port in docker-compose.yml |
| SQL Server timeout | Wait 60s for initialization |
| API not responding | Check logs: docker compose logs glms-backend-api |

## Environment Configuration

**Docker (docker-compose.yml):**
- API: ConnectionStrings__DefaultConnection=Server=sql-server-db;...
- Frontend: ApiSettings__BaseUrl=http://glms-backend-api:7001

**Local Development (appsettings.json):**
- API endpoint: https://localhost:7001

## Features

✅ **REST API** with proper HTTP verbs and status codes
✅ **Repository Pattern** for data abstraction
✅ **Dependency Injection** throughout
✅ **Swagger/OpenAPI** documentation
✅ **Docker Compose** orchestration
✅ **Multi-stage builds** for optimization
✅ **Health checks** for all containers
✅ **Integration tests** with in-memory database
✅ **JWT authentication** support
✅ **Environment consistency** via containerization


