using Microsoft.EntityFrameworkCore;
using Susurros_del_Cafe_WEB.Data;
using Susurros_del_Cafe_WEB.Services;
using Susurros_del_Cafe_WEB.Models;

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

// 🆕 CONFIGURACIÓN DE BASE DE DATOS PARA RAILWAY
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Usar PostgreSQL en producción, SQLite en desarrollo
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL")))
{
    // Producción - PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Desarrollo - SQLite
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=susurros_cafe.db"));
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

// Configurar EmailSettings
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

// Solo HTTPS en desarrollo
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
