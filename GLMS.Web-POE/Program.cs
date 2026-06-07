using GLMS.Web_POE.Data;
using Microsoft.EntityFrameworkCore;
using GLMS.Web_POE.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Register API services
builder.Services.AddHttpClient<IApiContractService, ApiContractService>(client =>
{
	var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
	client.BaseAddress = new Uri(apiBaseUrl);
	client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IApiClientService, ApiClientService>(client =>
{
	var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
	client.BaseAddress = new Uri(apiBaseUrl);
	client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Keep file service and currency service - they don't need API changes
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();

// For backward compatibility with existing services
// Only register SQL Server DbContext if not in test environment
if (!builder.Environment.EnvironmentName.Contains("Test"))
{
	builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Force invariant culture to prevent comma decimal separator issues
app.UseRequestLocalization(new RequestLocalizationOptions
{
	DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US"),
	SupportedCultures = new[] { new System.Globalization.CultureInfo("en-US") },
	SupportedUICultures = new[] { new System.Globalization.CultureInfo("en-US") }
});

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();