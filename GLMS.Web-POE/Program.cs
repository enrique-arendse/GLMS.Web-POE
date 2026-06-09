using GLMS.Web_POE.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://glms-backend-api:7001";

// Plain client used only to fetch JWT tokens (no auth handler attached)
builder.Services.AddHttpClient("ApiAuth", client =>
{
	client.BaseAddress = new Uri(apiBaseUrl);
	client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddTransient<ApiAuthTokenHandler>();

void RegisterAuthenticatedApiClient<TClient, TImplementation>()
	where TClient : class
	where TImplementation : class, TClient
{
	builder.Services.AddHttpClient<TClient, TImplementation>(client =>
	{
		client.BaseAddress = new Uri(apiBaseUrl);
		client.DefaultRequestHeaders.Add("Accept", "application/json");
	})
	.AddHttpMessageHandler<ApiAuthTokenHandler>();
}

RegisterAuthenticatedApiClient<IApiContractService, ApiContractService>();
RegisterAuthenticatedApiClient<IApiClientService, ApiClientService>();
RegisterAuthenticatedApiClient<IServiceRequest, ServiceRequestService>();

// Keep file service and currency service - they don't need API changes
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IFileService, FileService>();




var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}

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