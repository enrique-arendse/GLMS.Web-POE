using GLMS.Web_POE.Api.Repositories;
using GLMS.Web_POE.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Register Repositories for Dependency Injection
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen();

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowMvcApp", builder =>
	{
		builder.AllowAnyOrigin()
			   .AllowAnyMethod()
			   .AllowAnyHeader();
	});
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "default-secret-key-for-development-only-change-in-production");

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

// Apply migrations and seed data (guarded)
using (var scope = app.Services.CreateScope())
{
	try
	{
		var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		// Only attempt migrations for relational providers (avoid in-memory or missing DB issues)
		if (db.Database.IsRelational())
		{
			try
			{
				// Verify connection before attempting migration
				db.Database.GetDbConnection().Open();
				db.Database.Migrate();
				db.Database.GetDbConnection().Close();
			}
			catch (Exception ex) when (ex is Microsoft.Data.SqlClient.SqlException || ex is InvalidOperationException)
			{
				var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
				logger.LogWarning(ex, "Database unavailable; skipping automatic migrations.");
			}
		}
	}
	catch (Exception ex)
	{
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
		logger.LogWarning(ex, "An error occurred while applying migrations. Continuing without database migration.");
	}
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "GLMS API v1");
	});
}

app.UseHttpsRedirection();
app.UseCors("AllowMvcApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
