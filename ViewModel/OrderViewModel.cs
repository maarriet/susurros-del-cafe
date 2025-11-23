// ViewModels/OrderViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Susurros_del_Cafe_WEB.ViewModels
{
    public class OrderViewModel
    {
        // PRODUCTOS
        [Display(Name = "Café 250g")]
        public int Quantity250g { get; set; } = 0;

        [Display(Name = "Café 500g")]
        public int Quantity500g { get; set; } = 0;

        // INFORMACIÓN DEL CLIENTE
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre completo")]
        public string CustomerName { get; set; } = string.Empty;

        // 🆕 AGREGAR ESTA PROPIEDAD
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Display(Name = "Teléfono")]
        public string CustomerPhone { get; set; } = string.Empty;

        // INFORMACIÓN DE ENTREGA
        [Required(ErrorMessage = "La provincia es requerida")]
        [Display(Name = "Provincia")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "El método de pago es requerido")]
        [Display(Name = "Método de pago")]
        public int PaymentMethod { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [Display(Name = "Dirección completa")]
        public string CustomerAddress { get; set; } = string.Empty;

        [Display(Name = "Comentarios adicionales")]
        public string? Comments { get; set; }

        // PROPIEDADES CALCULADAS
        public decimal Subtotal => (Quantity250g * 3500) + (Quantity500g * 6500);
        public decimal ShippingCost => Province == "Alajuela" ? 0 : 1500;
        public decimal Total => Subtotal + ShippingCost;
        public bool HasProducts => Quantity250g > 0 || Quantity500g > 0;

        // MÉTODO PARA OBTENER ITEMS DEL PEDIDO
        public List<OrderItemViewModel> GetOrderItems()
        {
            var items = new List<OrderItemViewModel>();

            if (Quantity250g > 0)
            {
                items.Add(new OrderItemViewModel
                {
                    ProductId = 1,
                    ProductName = "Café Susurros 250g",
                    Quantity = Quantity250g,
                    UnitPrice = 3500,
                    Subtotal = Quantity250g * 3500
                });
            }

            if (Quantity500g > 0)
            {
                items.Add(new OrderItemViewModel
                {
                    ProductId = 2,
                    ProductName = "Café Susurros 500g",
                    Quantity = Quantity500g,
                    UnitPrice = 6500,
                    Subtotal = Quantity500g * 6500
                });
            }

            return items;
        }

    }


    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}