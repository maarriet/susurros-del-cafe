using Microsoft.AspNetCore.Mvc;
using Susurros_del_Cafe_WEB.Services;
using Susurros_del_Cafe_WEB.ViewModels;
using Susurros_del_Cafe_WEB.Models;

namespace Susurros_del_Cafe_WEB.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: Order/Create
        public IActionResult Create()
        {
            var model = new OrderViewModel();
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
                Console.WriteLine($"Provincia: '{model.Province}'");
                Console.WriteLine($"Dirección: '{model.CustomerAddress}'");
                Console.WriteLine($"Método de pago: {model.PaymentMethod}");
                Console.WriteLine($"Cantidad 250g: {model.Quantity250g}");
                Console.WriteLine($"Cantidad 500g: {model.Quantity500g}");
                Console.WriteLine($"HasProducts: {model.HasProducts}");
                Console.WriteLine($"Comentarios: '{model.Comments}'");

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
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception)
            {
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