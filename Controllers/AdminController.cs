// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Susurros_del_Cafe_WEB.Services;
using Susurros_del_Cafe_WEB.Models;


namespace Susurros_del_Cafe_WEB.Controllers
{
    public class AdminController : Controller
    {
        private readonly IOrderService _orderService;
        private const string ADMIN_PASSWORD = "SusurrosCafe2024"; // 🔐 Cambia esta contraseña

        public AdminController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // 🔐 PÁGINA DE LOGIN
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir al panel
            if (IsAuthenticated())
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // 🔐 VERIFICAR CONTRASEÑA
        [HttpPost]
        public IActionResult Login(string password)
        {
            if (password == ADMIN_PASSWORD)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Contraseña incorrecta";
            return View();
        }

        // 🔐 PROTEGIDO - Dashboard principal
        public async Task<IActionResult> Index()
        {
            // ✅ VERIFICAR AUTENTICACIÓN
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            try
            {
                var orders = await _orderService.GetOrdersAsync();
                return View(orders);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los pedidos: " + ex.Message;
                return View(new List<Order>());
            }
        }

        // 🔐 PROTEGIDO - Detalles del pedido
        public async Task<IActionResult> OrderDetails(int id)
        {
            // ✅ VERIFICAR AUTENTICACIÓN
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

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
                return RedirectToAction(nameof(Index));
            }
        }

        // 🔐 PROTEGIDO - Actualizar estado
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
        {
            // ✅ VERIFICAR AUTENTICACIÓN
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            try
            {
                await _orderService.UpdateOrderStatusAsync(orderId, status);
                TempData["Success"] = "Estado actualizado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error actualizando estado: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // 🔐 PROTEGIDO - Dashboard (redirect)
        public IActionResult Dashboard()
        {
            // ✅ VERIFICAR AUTENTICACIÓN
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            return RedirectToAction(nameof(Index));
        }

        // 🆕 NUEVO - Actualizar precios
        [HttpGet]
        public async Task<IActionResult> UpdatePrices()
        {
            // ✅ VERIFICAR AUTENTICACIÓN
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            try
            {
                await _orderService.UpdateProductPricesAsync();
                TempData["Success"] = "Precios actualizados correctamente a ₡2,500 (250g) y ₡4,500 (500g)";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error actualizando precios: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // 🆕 NUEVO - Cerrar sesión
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            TempData["Info"] = "Sesión cerrada correctamente";
            return RedirectToAction("Login");
        }

        // 🔐 MÉTODO AUXILIAR - Verificar autenticación
        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("IsAdmin") == "true";
        }
    }
}