using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Cüzdan_Uygulaması.Data;
using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.DataAccess;
using Cüzdan_Uygulaması.DataAccess.Interfaces;
using Cüzdan_Uygulaması.DataAccess.Repositories;
using Cüzdan_Uygulaması.BusinessLogic.Interfaces;
using Cüzdan_Uygulaması.BusinessLogic.Services;
using Cüzdan_Uygulaması.Middleware;
using Serilog;
using Serilog.Events;

// Configure Serilog from configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Cüzdan Uygulaması");

var builder = WebApplication.CreateBuilder(args);

// Use Serilog as the logging provider
builder.Host.UseSerilog();

// Add services to the container.

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Register Data Access Layer
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IInstallmentRepository, InstallmentRepository>();

// Register Business Logic Layer
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IInstallmentService, InstallmentService>();
builder.Services.AddScoped<ISimpleCategoryService, SimpleCategoryService>();
builder.Services.AddScoped<ICategoryInterestRateService, CategoryInterestRateService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IPdfService, PdfService>();

// Register Security and Logging Services
builder.Services.AddScoped<Cüzdan_Uygulaması.Services.ISecurityAuditService, Cüzdan_Uygulaması.Services.SecurityAuditService>();

builder.Services.AddControllersWithViews();

// Add Memory Caching
builder.Services.AddMemoryCache();

// Register Background Services
builder.Services.AddHostedService<Cüzdan_Uygulaması.Services.RecurringTransactionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Use request logging middleware first (before exception middleware)
app.UseMiddleware<RequestLoggingMiddleware>();

// Use custom global exception middleware instead of built-in exception handler
app.UseMiddleware<GlobalExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

Log.Information("Cüzdan Uygulaması started successfully");

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
