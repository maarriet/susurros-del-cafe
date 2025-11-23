using Microsoft.EntityFrameworkCore;
using Susurros_del_Cafe_WEB.Data;
using Susurros_del_Cafe_WEB.Services;
using Susurros_del_Cafe_WEB.Models;

var builder = WebApplication.CreateBuilder(args);

// 🆕 CONFIGURACIÓN PARA RAILWAY
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

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

// Configurar EmailSettings (si las usas)
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
    // No usar HTTPS en Railway
}

// No redirigir HTTPS en Railway para evitar problemas
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

Console.WriteLine($"🚀 Starting application on port {port}");
app.Run();
