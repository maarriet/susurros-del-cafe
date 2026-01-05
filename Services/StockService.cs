// Services/StockService.cs
using Microsoft.EntityFrameworkCore;
using Susurros_del_Cafe_WEB.Data;
using Susurros_del_Cafe_WEB.Models;

namespace Susurros_del_Cafe_WEB.Services
{
    public interface IStockService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<bool> UpdateProductAvailabilityAsync(int productId, bool IsActive);
        Task<bool> UpdateProductStockAsync(int productId, int quantity);
        Task InitializeProductsAsync();
    }

    public class StockService : IStockService
    {
        private readonly ApplicationDbContext _context;

        public StockService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> UpdateProductAvailabilityAsync(int productId, bool IsActive)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return false;

                product.IsActive = IsActive;
             

                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Producto {product.Name} actualizado: {(IsActive ? "Disponible" : "Agotado")}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error actualizando disponibilidad: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateProductStockAsync(int productId, int quantity)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return false;

                product.Stock = quantity;
              

                // Si el stock es 0, marcar como no disponible
                if (quantity == 0)
                {
                    product.IsActive = false;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error actualizando stock: {ex.Message}");
                return false;
            }
        }

        public async Task InitializeProductsAsync()
        {
            try
            {
                var productNames = new[]
                {
                    "Tueste Medio Molido 250g",
                    "Tueste Medio Molido 500g",
                    "Tueste Oscuro Molido 250g",
                    "Tueste Oscuro Molido 500g",
                    "Tueste Medio en Grano 250g",
                    "Tueste Medio en Grano 500g"
                };

                foreach (var productName in productNames)
                {
                    var existingProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.Name == productName);

                    if (existingProduct == null)
                    {
                        var price = productName.Contains("500g") ? 4500 : 2500;
                        var product = new Product
                        {
                            Name = productName,
                            Description = $"Café artesanal - {productName}",
                            Price = price,
                            IsActive = true,
                            Stock = 50
                        };

                        _context.Products.Add(product);
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Productos inicializados correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inicializando productos: {ex.Message}");
            }
        }
    }
}