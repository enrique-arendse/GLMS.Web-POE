using GLMS.Web_POE.Data;
using Microsoft.EntityFrameworkCore;
using GLMS.Web_POE.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();

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