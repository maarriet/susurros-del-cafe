using Microsoft.EntityFrameworkCore;
using Susurros_del_Cafe_WEB.Data;
using Susurros_del_Cafe_WEB.Services;
using Susurros_del_Cafe_WEB.Models;

// 🕐 CONFIGURAR TIMEZONE PARA POSTGRESQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// 🆕 CONFIGURACIÓN DE PUERTO PARA RAILWAY
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrEmpty(port) && int.TryParse(port, out int railwayPort))
    {
        serverOptions.ListenAnyIP(railwayPort);
        Console.WriteLine($"🚀 Railway port configured: {railwayPort}");
    }
    else
    {
        serverOptions.ListenAnyIP(5000);
        Console.WriteLine("🏠 Development port: 5000");
    }
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// 🆕 CONFIGURACIÓN DE BASE DE DATOS CON CONVERSIÓN DE URL
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Convertir URL de Railway a connection string de .NET
    var connectionString = ConvertDatabaseUrl(databaseUrl);
    
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
    
    Console.WriteLine("✅ Using Railway PostgreSQL with converted connection string");
}
else
{
    // Desarrollo local - SQLite
    var localConnection = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(localConnection ?? "Data Source=susurros_cafe.db"));
    
    Console.WriteLine("✅ Using local SQLite");
}

// Configurar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Registrar servicios
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

// 🆕 CREAR BASE DE DATOS EN RAILWAY
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL")))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            context.Database.EnsureCreated();
            Console.WriteLine("✅ Database created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database creation failed: {ex.Message}");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("🚀 Susurros del Café starting...");
app.Run();

// 🔧 FUNCIÓN PARA CONVERTIR URL DE RAILWAY
static string ConvertDatabaseUrl(string databaseUrl)
{
    try
    {
        var uri = new Uri(databaseUrl);
        var host = uri.Host;
        var port = uri.Port;
        var database = uri.LocalPath.TrimStart('/');
        var username = uri.UserInfo.Split(':')[0];
        var password = uri.UserInfo.Split(':')[1];

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error converting database URL: {ex.Message}");
        return databaseUrl; // Fallback al original
    }
}
