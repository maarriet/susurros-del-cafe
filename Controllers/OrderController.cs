using Microsoft.AspNetCore.Mvc;
using Susurros_del_Cafe_WEB.Services;
using Susurros_del_Cafe_WEB.ViewModels;
using Susurros_del_Cafe_WEB.Models;
using Susurros_del_Cafe_WEB.Data;

namespace Susurros_del_Cafe_WEB.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;

        public OrderController(IOrderService orderService, ApplicationDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        public IActionResult Create()
        {
            var model = new OrderViewModel();
            ViewBag.Products = _context.Products.Where(p => p.IsActive).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            try
            {
                Console.WriteLine($"=== 🔍 DEBUG COMPLETO DEL PEDIDO ===");
                Console.WriteLine($"Cliente: '{model.CustomerName}'");
                Console.WriteLine($"Teléfono: '{model.CustomerPhone}'");
                Console.WriteLine($"Email: '{model.CustomerEmail}'");
                Console.WriteLine($"Provincia: '{model.Province}'");
                Console.WriteLine($"Dirección: '{model.CustomerAddress}'");
                Console.WriteLine($"Método de pago: {model.PaymentMethod}");

                // 🆕 DEBUG PARA 6 PRODUCTOS
                Console.WriteLine($"=== 📦 PRODUCTOS SELECCIONADOS ===");
                Console.WriteLine($"Tueste Medio Molido 250g: {model.QuantityMedioMolido250g}");
                Console.WriteLine($"Tueste Medio Molido 500g: {model.QuantityMedioMolido500g}");
                Console.WriteLine($"Tueste Oscuro Molido 250g: {model.QuantityOscuroMolido250g}");
                Console.WriteLine($"Tueste Oscuro Molido 500g: {model.QuantityOscuroMolido500g}");
                Console.WriteLine($"Tueste Medio Grano 250g: {model.QuantityMedioGrano250g}");
                Console.WriteLine($"Tueste Medio Grano 500g: {model.QuantityMedioGrano500g}");

                Console.WriteLine($"HasProducts: {model.HasProducts}");
                Console.WriteLine($"Subtotal: ₡{model.Subtotal:N0}");
                Console.WriteLine($"Envío: ₡{model.ShippingCost:N0}");
                Console.WriteLine($"Total: ₡{model.Total:N0}");
                Console.WriteLine($"Comentarios: '{model.Comments}'");

                // 🆕 VALIDACIÓN DE STOCK ANTES DE PROCESAR
                var products = _context.Products.Where(p => p.IsActive).ToList();

                // Validar stock para cada producto seleccionado
                var stockErrors = new List<string>();

                if (model.QuantityMedioMolido250g > 0)
                {
                    var product = products.FirstOrDefault(p => p.Name == "Tueste Medio Molido 250g");
                    if (product == null || product.Stock == 0)
                        stockErrors.Add("Tueste Medio Molido 250g está agotado");
                    else if (model.QuantityMedioMolido250g > product.Stock)
                        stockErrors.Add($"Solo quedan {product.Stock} unidades de Tueste Medio Molido 250g");
                }

                if (model.QuantityMedioMolido500g > 0)
                {
                    var product = products.FirstOrDefault(p => p.Name == "Tueste Medio Molido 500g");
                    if (product == null || product.Stock == 0)
                        stockErrors.Add("Tueste Medio Molido 500g está agotado");
                    else if (model.QuantityMedioMolido500g > product.Stock)
                        stockErrors.Add($"Solo quedan {product.Stock} unidades de Tueste Medio Molido 500g");
                }

                if (model.QuantityOscuroMolido250g > 0)
                {
                    var product = products.FirstOrDefault(p => p.Name == "Tueste Oscuro Molido 250g");
                    if (product == null || product.Stock == 0)
                        stockErrors.Add("Tueste Oscuro Molido 250g está agotado");
                    else if (model.QuantityOscuroMolido250g > product.Stock)
                        stockErrors.Add($"Solo quedan {product.Stock} unidades de Tueste Oscuro Molido 250g");
                }

                if (model.QuantityOscuroMolido500g > 0)
                {
                    var product = products.FirstOrDefault(p => p.Name == "Tueste Oscuro Molido 500g");
                    if (product == null || product.Stock == 0)
                        stockErrors.Add("Tueste Oscuro Molido 500g está agotado");
                    else if (model.QuantityOscuroMolido500g > product.Stock)
                        stockErrors.Add($"Solo quedan {product.Stock} unidades de Tueste Oscuro Molido 500g");
                }

                if (model.QuantityMedioGrano250g > 0)
                {
                    var product = products.FirstOrDefault(p => p.Name == "Tueste Medio en Grano 250g");
                    if (product == null || product.Stock == 0)
                        stockErrors.Add("Tueste Medio en Grano 250g está agotado");
                    else if (model.QuantityMedioGrano250g > product.Stock)
                        stockErrors.Add($"Solo quedan {product.Stock} unidades de Tueste Medio en Grano 250g");
                }

                if (model.QuantityMedioGrano500g > 0)
                {
                    var product = products.FirstOrDefault(p => p.Name == "Tueste Medio en Grano 500g");
                    if (product == null || product.Stock == 0)
                        stockErrors.Add("Tueste Medio en Grano 500g está agotado");
                    else if (model.QuantityMedioGrano500g > product.Stock)
                        stockErrors.Add($"Solo quedan {product.Stock} unidades de Tueste Medio en Grano 500g");
                }

                // Si hay errores de stock, mostrarlos
                if (stockErrors.Any())
                {
                    foreach (var error in stockErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                    ViewBag.Products = products;
                    return View(model);
                }

                // Verificar ModelState
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("❌ ModelState NO ES VÁLIDO:");
                    foreach (var error in ModelState)
                    {
                        foreach (var errorMsg in error.Value.Errors)
                        {
                            Console.WriteLine($"   - Campo '{error.Key}': {errorMsg.ErrorMessage}");
                        }
                    }
                    ViewBag.Products = products;
                    return View(model);
                }
                else
                {
                    Console.WriteLine("✅ ModelState es válido");
                }

                // Verificar productos
                if (!model.HasProducts)
                {
                    Console.WriteLine("❌ No hay productos seleccionados");
                    ModelState.AddModelError("", "Debe seleccionar al menos un producto");
                    return View(model);
                }
                else
                {
                    Console.WriteLine("✅ Hay productos seleccionados");
                    Console.WriteLine($"📋 Resumen: {model.GetProductsSummary()}");
                }

                Console.WriteLine("🔄 Llamando a CreateOrderAsync...");
                var order = await _orderService.CreateOrderAsync(model);
                Console.WriteLine($"✅ Pedido creado exitosamente con ID: {order.Id}");

                return RedirectToAction("Confirmation", new { id = order.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EXCEPCIÓN EN OrderController.Create:");
                Console.WriteLine($"   Mensaje: {ex.Message}");
                Console.WriteLine($"   Tipo: {ex.GetType().Name}");
                Console.WriteLine($"   StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   InnerException: {ex.InnerException.Message}");
                }

                ModelState.AddModelError("", $"Error específico: {ex.Message}");
                ViewBag.Products = _context.Products.Where(p => p.IsActive).ToList();
                return View(model);
            }
        }

        // GET: Order/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    Console.WriteLine($"❌ No se encontró pedido con ID: {id}");
                    return NotFound();
                }

                Console.WriteLine($"✅ Mostrando confirmación para pedido ID: {id}");
                return View(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error obteniendo pedido {id}: {ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}