using Autofac;
using Autofac.Extensions.DependencyInjection;
using DotNetEnv;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Contracts;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.Components;
using TheFamilyDaybook.Web.Services;

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

// Add Razor Pages for traditional form POST (needed for authentication)
builder.Services.AddRazorPages();

// Add HttpContextAccessor for authentication state provider
// This must be registered before the authentication state provider
builder.Services.AddHttpContextAccessor();

// Configure Entity Framework with PostgreSQL
// Using Npgsql provider for PostgreSQL (configured in docker-compose.yml)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' or environment variable 'CONNECTION_STRING' not found.");

// Register DbContextFactory first (for use in Blazor components)
// The factory is a singleton but creates scoped DbContext instances
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register scoped DbContext for Identity and other services
// This must be registered before AddIdentity
// Using the same connection string but separate registration to avoid scope issues
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString), ServiceLifetime.Scoped);

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure authentication cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Note: Repositories and application services are registered in Autofac below
// This allows Autofac to handle scope resolution better than built-in DI

// Configure Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Register services with Autofac
    // Autofac will handle scope resolution better than built-in DI
    containerBuilder.RegisterType<RevalidatingIdentityAuthenticationStateProvider>()
        .As<AuthenticationStateProvider>()
        .InstancePerLifetimeScope();
    
    containerBuilder.RegisterType<AccountService>()
        .As<IAccountService>()
        .InstancePerLifetimeScope();
    
    containerBuilder.RegisterType<StudentService>()
        .As<IStudentService>()
        .InstancePerLifetimeScope();
    
    containerBuilder.RegisterType<SubjectService>()
        .As<ISubjectService>()
        .InstancePerLifetimeScope();
    
    containerBuilder.RegisterType<MetricService>()
        .As<IMetricService>()
        .InstancePerLifetimeScope();
    
    containerBuilder.RegisterType<StudentSubjectService>()
        .As<IStudentSubjectService>()
        .InstancePerLifetimeScope();
    
    containerBuilder.RegisterType<StudentMetricService>()
        .As<IStudentMetricService>()
        .InstancePerLifetimeScope();
    
    containerBuilder.RegisterType<StudentSubjectMetricService>()
        .As<IStudentSubjectMetricService>()
        .InstancePerLifetimeScope();
    
    containerBuilder.RegisterType<DailyLogService>()
        .As<IDailyLogService>()
        .InstancePerLifetimeScope();
    
    // Register generic repository
    containerBuilder.RegisterGeneric(typeof(Repository<>))
        .As(typeof(IRepository<>))
        .InstancePerLifetimeScope();
});

var app = builder.Build();

// Test database connection at startup (optional - remove if you want lazy connection)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            Console.WriteLine("✓ Database connection successful");
        }
    }
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

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorPages(); // Map Razor Pages (for Login.cshtml)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
