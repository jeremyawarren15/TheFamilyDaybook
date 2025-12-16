using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Contracts;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Web.Components;

// Load .env file if it exists (for local development)
// Check in current directory first, then solution root (two levels up)
var currentDir = Directory.GetCurrentDirectory();
var envPath = Path.Combine(currentDir, ".env");
if (!File.Exists(envPath))
{
    // Try solution root (two levels up from Web project)
    var solutionRoot = Path.Combine(currentDir, "..", "..");
    envPath = Path.GetFullPath(Path.Combine(solutionRoot, ".env"));
}
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure Entity Framework with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' or environment variable 'CONNECTION_STRING' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var app = builder.Build();

// Test database connection at startup (optional - remove if you want lazy connection)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.CanConnect(); // This will throw if connection fails
    }
    Console.WriteLine("✓ Database connection successful");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠ Warning: Database connection failed: {ex.Message}");
    Console.WriteLine("⚠ Application will start, but database operations will fail until connection is restored.");
    // Uncomment the line below if you want the app to fail to start when DB is unavailable:
    // throw;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
