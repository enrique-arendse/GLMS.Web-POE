using GLMS.Web_POE.Api.Repositories;
using GLMS.Web_POE.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// Add Services
// ========================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new() { Title = "GLMS API", Version = "v1" });

	c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "Enter: Bearer {your token here}"
	});

	c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});

// ========================================
// Database Configuration
// ========================================

if (builder.Environment.IsEnvironment("Test"))
{
	builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseInMemoryDatabase("IntegrationTestDb"));
}
else
{
	builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseSqlServer(
			builder.Configuration.GetConnectionString("DefaultConnection")
		));
}

// ========================================
// Repository Dependency Injection
// ========================================

builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();

// ========================================
// CORS Configuration
// ========================================

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowMvcApp", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

// ========================================
// JWT Authentication
// ========================================

var jwtSettings = builder.Configuration.GetSection("JwtSettings");

var key = Encoding.ASCII.GetBytes(
	jwtSettings["SecretKey"] ??
	"default-secret-key-for-development-only-change-this"
);

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key),

		ValidateIssuer = false,
		ValidateAudience = false,

		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero
	};
});

var app = builder.Build();

// ========================================
// Apply EF Core Migrations Automatically
// ========================================

if (!app.Environment.IsEnvironment("Test"))
{
	using var scope = app.Services.CreateScope();
	var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

	try
	{
		var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		logger.LogInformation("Applying database migrations...");

		db.Database.Migrate();

		logger.LogInformation("Database migrations applied successfully.");
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "An error occurred while applying migrations.");
	}
}

// ========================================
// Configure Middleware Pipeline
// ========================================

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
	app.UseSwagger();

	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "GLMS API v1");
	});
}

app.UseCors("AllowMvcApp");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

