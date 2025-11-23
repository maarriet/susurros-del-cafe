// Program.cs
using Susurros_del_Cafe_WEB.Data;
using Susurros_del_Cafe_WEB.Models;
using Susurros_del_Cafe_WEB.Services;
using Microsoft.EntityFrameworkCore;

// Add services to the container
builder.Services.AddControllersWithViews();
// 🆕 CONFIGURACIÓN PARA RAILWAY
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var builder = WebApplication.CreateBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");



// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


// Your custom services
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEmailService, EmailService>();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


// ✅ ESTA SECCIÓN ES CRÍTICA PARA CREAR LA DB
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        Console.WriteLine("🗄️ Intentando crear base de datos...");

        // Crear la base de datos y todas las tablas
        bool created = context.Database.EnsureCreated();

        if (created)
        {
            Console.WriteLine("✅ Base de datos creada exitosamente");
        }
        else
        {
            Console.WriteLine("✅ Base de datos ya existe");
        }

        // Verificar si hay productos, si no, crear algunos
        if (!context.Products.Any())
        {
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Café Susurros 250g",
                    Price = 2500,
                    Description = "Café artesanal 250g - Perfecto para disfrutar en casa"
                },
                new Product
                {
                    Name = "Café Susurros 500g",
                    Price = 4500,
                    Description = "Café artesanal 500g - Ideal para familias cafeteras"
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
            Console.WriteLine($"✅ Se crearon {products.Count} productos iniciales");
        }
        else
        {
            Console.WriteLine($"✅ Ya existen {context.Products.Count()} productos");
        }

        // Verificar que el archivo se creó
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "susurros_cafe.db");
        if (File.Exists(dbPath))
        {
            Console.WriteLine($"✅ Archivo de base de datos confirmado en: {dbPath}");
        }
        else
        {
            Console.WriteLine($"❌ Archivo de base de datos NO encontrado en: {dbPath}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error inicializando DB: {ex.Message}");
        Console.WriteLine($"Detalles: {ex.InnerException?.Message}");
        Console.WriteLine($"StackTrace: {ex.StackTrace}");
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("🚀 Susurros del Café iniciando...");
app.Run();
