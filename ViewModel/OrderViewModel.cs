// ViewModels/OrderViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Susurros_del_Cafe_WEB.ViewModels
{
    public class OrderViewModel
    {
        // PRODUCTOS - 6 VARIEDADES
        [Display(Name = "Tueste Medio Molido 250g")]
        public int QuantityMedioMolido250g { get; set; } = 0;

        [Display(Name = "Tueste Medio Molido 500g")]
        public int QuantityMedioMolido500g { get; set; } = 0;

        [Display(Name = "Tueste Oscuro Molido 250g")]
        public int QuantityOscuroMolido250g { get; set; } = 0;

        [Display(Name = "Tueste Oscuro Molido 500g")]
        public int QuantityOscuroMolido500g { get; set; } = 0;

        [Display(Name = "Tueste Medio en Grano 250g")]
        public int QuantityMedioGrano250g { get; set; } = 0;

        [Display(Name = "Tueste Medio en Grano 500g")]
        public int QuantityMedioGrano500g { get; set; } = 0;

        // INFORMACIÓN DEL CLIENTE
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre completo")]
        public string CustomerName { get; set; } = string.Empty;

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

        // PROPIEDADES CALCULADAS - ACTUALIZADAS PARA 6 PRODUCTOS
        public decimal Subtotal =>
            (QuantityMedioMolido250g * 2500) +
            (QuantityMedioMolido500g * 4500) +
            (QuantityOscuroMolido250g * 2500) +
            (QuantityOscuroMolido500g * 4500) +
            (QuantityMedioGrano250g * 2500) +
            (QuantityMedioGrano500g * 4500);

        public decimal ShippingCost => Province == "Alajuela" ? 0 : 3200; // CORREGIDO A 3200

        public decimal Total => Subtotal + ShippingCost;

        public bool HasProducts =>
            QuantityMedioMolido250g > 0 ||
            QuantityMedioMolido500g > 0 ||
            QuantityOscuroMolido250g > 0 ||
            QuantityOscuroMolido500g > 0 ||
            QuantityMedioGrano250g > 0 ||
            QuantityMedioGrano500g > 0;

        // MÉTODO PARA OBTENER ITEMS DEL PEDIDO - ACTUALIZADO
        public List<OrderItemViewModel> GetOrderItems()
        {
            var items = new List<OrderItemViewModel>();

            if (QuantityMedioMolido250g > 0)
            {
                items.Add(new OrderItemViewModel
                {
                    ProductId = 1,
                    ProductName = "Tueste Medio Molido 250g",
                    Quantity = QuantityMedioMolido250g,
                    UnitPrice = 2500,
                    Subtotal = QuantityMedioMolido250g * 2500
                });
            }

            if (QuantityMedioMolido500g > 0)
            {
                items.Add(new OrderItemViewModel
                {
                    ProductId = 2,
                    ProductName = "Tueste Medio Molido 500g",
                    Quantity = QuantityMedioMolido500g,
                    UnitPrice = 4500,
                    Subtotal = QuantityMedioMolido500g * 4500
                });
            }

            if (QuantityOscuroMolido250g > 0)
            {
                items.Add(new OrderItemViewModel
                {
                    ProductId = 3,
                    ProductName = "Tueste Oscuro Molido 250g",
                    Quantity = QuantityOscuroMolido250g,
                    UnitPrice = 2500,
                    Subtotal = QuantityOscuroMolido250g * 2500
                });
            }

            if (QuantityOscuroMolido500g > 0)
            {
                items.Add(new OrderItemViewModel
                {
                    ProductId = 4,
                    ProductName = "Tueste Oscuro Molido 500g",
                    Quantity = QuantityOscuroMolido500g,
                    UnitPrice = 4500,
                    Subtotal = QuantityOscuroMolido500g * 4500
                });
            }

            if (QuantityMedioGrano250g > 0)
            {
                items.Add(new OrderItemViewModel
                {
                    ProductId = 5,
                    ProductName = "Tueste Medio en Grano 250g",
                    Quantity = QuantityMedioGrano250g,
                    UnitPrice = 2500,
                    Subtotal = QuantityMedioGrano250g * 2500
                });
            }

            if (QuantityMedioGrano500g > 0)
            {
                items.Add(new OrderItemViewModel
                {
                    ProductId = 6,
                    ProductName = "Tueste Medio en Grano 500g",
                    Quantity = QuantityMedioGrano500g,
                    UnitPrice = 4500,
                    Subtotal = QuantityMedioGrano500g * 4500
                });
            }

            return items;
        }

        // 🆕 MÉTODO PARA OBTENER RESUMEN DE PRODUCTOS SELECCIONADOS
        public string GetProductsSummary()
        {
            var products = new List<string>();

            if (QuantityMedioMolido250g > 0)
                products.Add($"{QuantityMedioMolido250g}x Tueste Medio Molido 250g");
            if (QuantityMedioMolido500g > 0)
                products.Add($"{QuantityMedioMolido500g}x Tueste Medio Molido 500g");
            if (QuantityOscuroMolido250g > 0)
                products.Add($"{QuantityOscuroMolido250g}x Tueste Oscuro Molido 250g");
            if (QuantityOscuroMolido500g > 0)
                products.Add($"{QuantityOscuroMolido500g}x Tueste Oscuro Molido 500g");
            if (QuantityMedioGrano250g > 0)
                products.Add($"{QuantityMedioGrano250g}x Tueste Medio Grano 250g");
            if (QuantityMedioGrano500g > 0)
                products.Add($"{QuantityMedioGrano500g}x Tueste Medio Grano 500g");

            return string.Join(", ", products);
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