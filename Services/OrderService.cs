using Microsoft.EntityFrameworkCore;
using Susurros_del_Cafe_WEB.Data;
using Susurros_del_Cafe_WEB.Models;
using Susurros_del_Cafe_WEB.ViewModels;

namespace Susurros_del_Cafe_WEB.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(OrderViewModel orderViewModel);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<List<Order>> GetOrdersAsync();
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task UpdateProductPricesAsync();
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public OrderService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<Order> CreateOrderAsync(OrderViewModel orderViewModel)
        {
            try
            {
                Console.WriteLine($"🔄 OrderService.CreateOrderAsync iniciado");
                Console.WriteLine($"📝 Datos recibidos:");
                Console.WriteLine($"   - Cliente: {orderViewModel.CustomerName}");
                Console.WriteLine($"   - Teléfono: {orderViewModel.CustomerPhone}");
                Console.WriteLine($"   - Provincia: '{orderViewModel.Province}'"); // ✅ Verificar que llega
                Console.WriteLine($"   - Dirección: '{orderViewModel.CustomerAddress}'");

                // Verificar conexión a DB
                var canConnect = await _context.Database.CanConnectAsync();
                Console.WriteLine($"🗄️ Conexión DB: {(canConnect ? "✅ OK" : "❌ FALLO")}");

                if (!canConnect)
                {
                    throw new Exception("No se puede conectar a la base de datos");
                }

                if (!orderViewModel.HasProducts)
                {
                    throw new InvalidOperationException("No hay productos seleccionados");
                }

                Console.WriteLine($"👤 Creando cliente...");

                // ✅ ASEGURAR QUE PROVINCE SE ASIGNE CORRECTAMENTE
                var customer = new Customer
                {
                    Name = orderViewModel.CustomerName,
                    Email = orderViewModel.CustomerEmail,
                    Phone = orderViewModel.CustomerPhone,
                    Address = orderViewModel.CustomerAddress,
                    Province = orderViewModel.Province ?? "No especificada" // ✅ AGREGAR ESTA LÍNEA
                };

                // ✅ VERIFICAR ANTES DE GUARDAR
                Console.WriteLine($"📋 Verificando datos del cliente antes de guardar:");
                Console.WriteLine($"   - Name: '{customer.Name}'");
                Console.WriteLine($"   - Phone: '{customer.Phone}'");
                Console.WriteLine($"   - Address: '{customer.Address}'");
                Console.WriteLine($"   - Province: '{customer.Province}'");
                Console.WriteLine($"   - Email: '{customer.Email}'");

                _context.Customers.Add(customer);

                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ Cliente creado con ID: {customer.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ ERROR AL GUARDAR CLIENTE:");
                    Console.WriteLine($"   Mensaje: {ex.Message}");
                    Console.WriteLine($"   InnerException: {ex.InnerException?.Message}");
                    throw new Exception($"Error creando cliente: {ex.InnerException?.Message ?? ex.Message}");
                }

                // Resto del código igual...
                Console.WriteLine($"📦 Creando pedido...");
                var order = new Order
                {
                    CustomerId = customer.Id,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending,
                    OrderItems = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                // Producto 250g
                if (orderViewModel.Quantity250g > 0)
                {
                    Console.WriteLine($"📦 Procesando producto 250g...");
                    var product250 = await GetOrCreateProductSafe("Café Susurros 250g", 2500, "Café artesanal 250g");

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = product250.Id,
                        Quantity = orderViewModel.Quantity250g,
                        UnitPrice = 2500
                    });

                    totalAmount += orderViewModel.Quantity250g * 2500;
                    Console.WriteLine($"✅ Producto 250g agregado");
                }

                // Producto 500g
                if (orderViewModel.Quantity500g > 0)
                {
                    Console.WriteLine($"📦 Procesando producto 500g...");
                    var product500 = await GetOrCreateProductSafe("Café Susurros 500g", 4500, "Café artesanal 500g");

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = product500.Id,
                        Quantity = orderViewModel.Quantity500g,
                        UnitPrice = 4500
                    });

                    totalAmount += orderViewModel.Quantity500g * 4500;
                    Console.WriteLine($"✅ Producto 500g agregado");
                }

                // Costo de envío
                decimal shippingCost = orderViewModel.Province == "Alajuela" ? 0 : 3200;
                totalAmount += shippingCost;

                Console.WriteLine($"💰 Total calculado: ₡{totalAmount:N0}");

                // Asignar propiedades opcionales
                SetOptionalProperties(order, totalAmount, orderViewModel.Comments);

                Console.WriteLine($"💾 Guardando pedido en base de datos...");
                _context.Orders.Add(order);

                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ Pedido guardado con ID: {order.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ ERROR AL GUARDAR PEDIDO:");
                    Console.WriteLine($"   Mensaje: {ex.Message}");
                    Console.WriteLine($"   InnerException: {ex.InnerException?.Message}");
                    throw new Exception($"Error guardando pedido: {ex.InnerException?.Message ?? ex.Message}");
                }

                // Email (no crítico)
                try
                {
                    //await _emailService.SendOrderConfirmationAsync(order);
                    Console.WriteLine($"📧 Email enviado correctamente");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error enviando email (no crítico): {ex.Message}");
                }

                Console.WriteLine($"🎉 CreateOrderAsync completado exitosamente");
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EXCEPCIÓN EN OrderService.CreateOrderAsync:");
                Console.WriteLine($"   Mensaje: {ex.Message}");
                Console.WriteLine($"   InnerException: {ex.InnerException?.Message}");
                Console.WriteLine($"   StackTrace: {ex.StackTrace}");
                throw;
            }
        }
        // ✅ MÉTODO SEGURO PARA CREAR/OBTENER PRODUCTOS
        private async Task<Product> GetOrCreateProductSafe(string name, decimal price, string description)
        {
            try
            {
                Console.WriteLine($"🔍 Buscando producto: {name}");
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == name);

                if (product == null)
                {
                    Console.WriteLine($"📦 Creando nuevo producto: {name}");
                    product = new Product
                    {
                        Name = name,
                        Price = price,
                        Description = description
                    };
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ Producto creado con ID: {product.Id}");
                }
                else
                {
                    Console.WriteLine($"✅ Producto encontrado con ID: {product.Id}");
                }

                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR CREANDO/OBTENIENDO PRODUCTO {name}:");
                Console.WriteLine($"   Mensaje: {ex.Message}");
                Console.WriteLine($"   InnerException: {ex.InnerException?.Message}");
                throw new Exception($"Error con producto {name}: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        private void SetOptionalProperties(Order order, decimal totalAmount, string? comments)
        {
            try
            {
                // Asignar Total si la propiedad existe
                var totalProperty = order.GetType().GetProperty("Total");
                if (totalProperty == null)
                {
                    totalProperty = order.GetType().GetProperty("TotalAmount");
                }

                if (totalProperty != null)
                {
                    totalProperty.SetValue(order, totalAmount);
                    Console.WriteLine($"✅ Total asignado: ₡{totalAmount:N0}");
                }
                else
                {
                    Console.WriteLine($"⚠️ No se encontró propiedad Total/TotalAmount en Order");
                }

                // Asignar comentarios si la propiedad existe
                var notesProperty = order.GetType().GetProperty("Comments");
                if (notesProperty == null)
                {
                    notesProperty = order.GetType().GetProperty("Notes");
                }

                if (notesProperty != null)
                {
                    notesProperty.SetValue(order, comments);
                    Console.WriteLine($"✅ Comentarios asignados");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error asignando propiedades opcionales: {ex.Message}");
            }
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ MÉTODOS AUXILIARES
        private async Task<Product> GetOrCreateProduct(string name, decimal price, string description)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == name);
            if (product == null)
            {
                product = new Product
                {
                    Name = name,
                    Price = price,
                    Description = description
                };
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }
            return product;
        }

        private bool HasTotalAmountProperty(Order order)
        {
            return order.GetType().GetProperty("TotalAmount") != null;
        }

        private bool HasNotesProperty(Order order)
        {
            return order.GetType().GetProperty("Notes") != null;
        }

        private void SetTotalAmount(Order order, decimal amount)
        {
            var property = order.GetType().GetProperty("TotalAmount");
            property?.SetValue(order, amount);
        }

        private void SetNotes(Order order, string? notes)
        {
            var property = order.GetType().GetProperty("Notes");
            property?.SetValue(order, notes);
        }


        // 🆕 NUEVO MÉTODO - Actualizar precios de productos
        public async Task UpdateProductPricesAsync()
        {
            try
            {
                Console.WriteLine("🔄 Actualizando precios de productos...");

                // Buscar productos existentes por nombre
                var product250 = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name.Contains("250g"));
                var product500 = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name.Contains("500g"));

                bool updated = false;

                // Actualizar producto 250g
                if (product250 != null && product250.Price != 2500)
                {
                    Console.WriteLine($"📦 Actualizando precio 250g: ₡{product250.Price:N0} → ₡2,500");
                    product250.Price = 2500;
                    updated = true;
                }
                else if (product250 != null)
                {
                    Console.WriteLine($"✅ Producto 250g ya tiene el precio correcto: ₡{product250.Price:N0}");
                }

                // Actualizar producto 500g
                if (product500 != null && product500.Price != 4500)
                {
                    Console.WriteLine($"📦 Actualizando precio 500g: ₡{product500.Price:N0} → ₡4,500");
                    product500.Price = 4500;
                    updated = true;
                }
                else if (product500 != null)
                {
                    Console.WriteLine($"✅ Producto 500g ya tiene el precio correcto: ₡{product500.Price:N0}");
                }

                // Guardar cambios si hubo actualizaciones
                if (updated)
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine("✅ Precios actualizados correctamente en la base de datos");
                }
                else
                {
                    Console.WriteLine("ℹ️ Todos los precios ya están actualizados");
                }

                // Verificar si no existen productos
                if (product250 == null && product500 == null)
                {
                    Console.WriteLine("⚠️ No se encontraron productos para actualizar");
                    Console.WriteLine("💡 Los productos se crearán automáticamente con el próximo pedido");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error actualizando precios: {ex.Message}");
                Console.WriteLine($"   InnerException: {ex.InnerException?.Message}");
                throw new Exception($"Error actualizando precios: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

    } // ← Cierre de la clase OrderService
}

