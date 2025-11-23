// Services/EmailService.cs
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Susurros_del_Cafe_WEB.Models;
using Microsoft.Extensions.Options;

namespace Susurros_del_Cafe_WEB.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendOrderConfirmationAsync(Order order)
        {
            try
            {
                var subject = $"Confirmación de Pedido #{order.Id} - Susurros del Café";
                var body = GenerateOrderConfirmationHtml(order);

                // 🆕 ENVIAR AL EMAIL DEL CLIENTE
                if (!string.IsNullOrEmpty(order.Customer.Email) && order.Customer.Email.Contains("@"))
                {
                    await SendEmailAsync(order.Customer.Email, subject, body);
                    _logger.LogInformation("Confirmation email sent to customer: {Email}", order.Customer.Email);
                }

                // Enviar notificación al admin
                var adminSubject = $"Nuevo Pedido #{order.Id} - {order.Customer.Name}";
                var adminBody = GenerateAdminNotificationHtml(order);
                await SendEmailAsync(_emailSettings.AdminEmail, adminSubject, adminBody);

                _logger.LogInformation("Order confirmation emails sent for Order #{OrderId}", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order confirmation email for order {OrderId}", order.Id);
                // Don't throw - we don't want email failures to break order creation
            }
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Susurros del Café", _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                // Connect to the SMTP server
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port,
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                // Authenticate if credentials are provided
                if (!string.IsNullOrEmpty(_emailSettings.Username))
                {
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                }

                // Send the message
                await client.SendAsync(message);

                _logger.LogInformation("Email sent successfully to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        private string GenerateOrderConfirmationHtml(Order order)
        {
            var itemsHtml = "";
            foreach (var item in order.OrderItems)
            {
                itemsHtml += $@"
                    <tr>
                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{item.Product.Name}</td>
                        <td style='padding: 8px; border-bottom: 1px solid #eee; text-align: center;'>{item.Quantity}</td>
                        <td style='padding: 8px; border-bottom: 1px solid #eee; text-align: right;'>₡{item.UnitPrice:N0}</td>
                        <td style='padding: 8px; border-bottom: 1px solid #eee; text-align: right;'>₡{(item.Quantity * item.UnitPrice):N0}</td>
                    </tr>";
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Confirmación de Pedido - Susurros del Café</title>
    <style>
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            line-height: 1.6; 
            color: #333; 
            margin: 0; 
            padding: 0; 
            background-color: #f4f4f4; 
        }}
        .container {{ 
            max-width: 600px; 
            margin: 0 auto; 
            background-color: #ffffff; 
            box-shadow: 0 0 10px rgba(0,0,0,0.1); 
        }}
        .header {{ 
            background: linear-gradient(135deg, #8B4513, #A0522D); 
            color: white; 
            padding: 30px 20px; 
            text-align: center; 
        }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .header h2 {{ margin: 10px 0 0 0; font-size: 18px; font-weight: normal; opacity: 0.9; }}
        .content {{ padding: 30px 20px; }}
        .order-summary {{ 
            background-color: #f8f9fa; 
            border-radius: 8px; 
            padding: 20px; 
            margin: 20px 0; 
            border-left: 4px solid #8B4513; 
        }}
        .order-table {{ 
            width: 100%; 
            border-collapse: collapse; 
            margin: 15px 0; 
        }}
        .order-table th {{ 
            background-color: #8B4513; 
            color: white; 
            padding: 12px 8px; 
            text-align: left; 
        }}
        .total-row {{ 
            font-weight: bold; 
            font-size: 1.1em; 
            background-color: #e8f5e8; 
        }}
        .info-section {{ 
            background-color: #fff; 
            border: 1px solid #dee2e6; 
            border-radius: 6px; 
            padding: 15px; 
            margin: 15px 0; 
        }}
        .info-section h4 {{ 
            margin-top: 0; 
            color: #8B4513; 
            border-bottom: 2px solid #8B4513; 
            padding-bottom: 5px; 
        }}
        .footer {{ 
            background-color: #2c3e50; 
            color: white; 
            text-align: center; 
            padding: 20px; 
            font-size: 14px; 
        }}
        .status-badge {{ 
            display: inline-block; 
            padding: 4px 12px; 
            border-radius: 20px; 
            background-color: #ffc107; 
            color: #000; 
            font-weight: bold; 
            font-size: 12px; 
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>¡Gracias por tu pedido!</h1>
            <h2>Susurros del Café</h2>
        </div>
        
        <div class='content'>
            <h3>Hola {order.Customer.Name},</h3>
            <p>Hemos recibido tu pedido y lo estamos preparando con mucho cariño. A continuación encontrarás los detalles de tu compra:</p>
            
            <div class='order-summary'>
                <h4>📋 Pedido #{order.Id}</h4>
                <p><strong>Fecha:</strong> {order.OrderDate:dddd, dd 'de' MMMM 'de' yyyy 'a las' HH:mm}</p>
                <p><strong>Estado:</strong> <span class='status-badge'>{GetStatusText(order.Status)}</span></p>
                
                <table class='order-table'>
                    <thead>
                        <tr>
                            <th>Producto</th>
                            <th style='text-align: center;'>Cantidad</th>
                            <th style='text-align: right;'>Precio Unit.</th>
                            <th style='text-align: right;'>Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                        <tr style='border-top: 2px solid #8B4513;'>
                            <td colspan='3' style='padding: 12px 8px; font-weight: bold;'>Subtotal:</td>
                            <td style='padding: 12px 8px; text-align: right; font-weight: bold;'>₡{order.Subtotal:N0}</td>
                        </tr>
                        <tr>
                            <td colspan='3' style='padding: 8px; font-weight: bold;'>Envío:</td>
                            <td style='padding: 8px; text-align: right; font-weight: bold;'>
                                {(order.ShippingCost == 0 ? "¡GRATIS!" : $"₡{order.ShippingCost:N0}")}
                            </td>
                        </tr>
                        <tr class='total-row'>
                            <td colspan='3' style='padding: 15px 8px; font-size: 1.2em;'>TOTAL:</td>
                            <td style='padding: 15px 8px; text-align: right; font-size: 1.2em; color: #8B4513;'>₡{order.Total:N0}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
            
            <div class='info-section'>
                <h4>📍 Información de Entrega</h4>
                <p><strong>Dirección:</strong> {order.Customer.Address}</p>
                <p><strong>Provincia:</strong> {order.Customer.Province}</p>
                <p><strong>Teléfono:</strong> {order.Customer.Phone}</p>
                <p><strong>Método de Pago:</strong> {GetPaymentMethodText(order.PaymentMethod)}</p>
                {(string.IsNullOrEmpty(order.Comments) ? "" : $"<p><strong>Comentarios:</strong> {order.Comments}</p>")}
            </div>
            
            <div class='info-section'>
                <h4>📞 Próximos Pasos</h4>
                <p>• Te contactaremos en las próximas horas para confirmar tu pedido</p>
                <p>• Coordinaremos la entrega según tu ubicación</p>
                <p>• {(order.ShippingCost == 0 ? "¡Disfruta del envío gratuito a Alajuela!" : "Tu pedido será enviado por Correos de Costa Rica")}</p>
            </div>
            
            <p style='text-align: center; margin-top: 30px; font-style: italic; color: #666;'>
                ¡Gracias por confiar en Susurros del Café para endulzar tus momentos especiales!
            </p>
        </div>
        
        <div class='footer'>
            <p><strong>Susurros del Café</strong><br>
            El mejor café artesanal de Costa Rica</p>
            <p>📱 WhatsApp: +506 8888-8888 | 📧 Email: info@susurrosdelcafe.com</p>
            <p style='font-size: 12px; opacity: 0.8;'>
                Si tienes alguna pregunta sobre tu pedido, no dudes en contactarnos.
            </p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateAdminNotificationHtml(Order order)
        {
            var itemsList = "";
            foreach (var item in order.OrderItems)
            {
                itemsList += $"<li>{item.Product.Name} x{item.Quantity} - ₡{(item.Quantity * item.UnitPrice):N0}</li>";
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
        .alert {{ 
            background: linear-gradient(135deg, #d4edda, #c3e6cb); 
            border: 2px solid #28a745; 
            padding: 20px; 
            border-radius: 8px; 
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .header {{ color: #155724; font-size: 24px; margin-bottom: 15px; }}
        .details {{ background-color: white; padding: 15px; border-radius: 5px; margin: 10px 0; }}
        .urgent {{ color: #dc3545; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='alert'>
        <div class='header'>🆕 ¡NUEVO PEDIDO RECIBIDO!</div>
        
        <div class='details'>
            <h3>Información del Pedido</h3>
            <p><strong>Pedido #:</strong> {order.Id}</p>
            <p><strong>Fecha:</strong> {order.OrderDate:dd/MM/yyyy HH:mm}</p>
            <p class='urgent'><strong>Total:</strong> ₡{order.Total:N0}</p>
            
            <h4>Cliente:</h4>
            <p><strong>Nombre:</strong> {order.Customer.Name}</p>
            <p><strong>Teléfono:</strong> {order.Customer.Phone}</p>
            <p><strong>Dirección:</strong> {order.Customer.Address}</p>
            <p><strong>Provincia:</strong> {order.Customer.Province}</p>
            
            <h4>Productos:</h4>
            <ul>{itemsList}</ul>
            
            <p><strong>Método de Pago:</strong> {GetPaymentMethodText(order.PaymentMethod)}</p>
            <p><strong>Costo de Envío:</strong> {(order.ShippingCost == 0 ? "GRATIS (Alajuela)" : $"₡{order.ShippingCost:N0}")}</p>
            
            {(string.IsNullOrEmpty(order.Comments) ? "" : $"<p><strong>Comentarios del Cliente:</strong> <em>{order.Comments}</em></p>")}
        </div>
        
        <p style='text-align: center; margin-top: 20px;'>
            <strong>👉 Revisa el panel de administración para gestionar este pedido</strong>
        </p>
    </div>
</body>
</html>";
        }

        private string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Pendiente",
                OrderStatus.Processing => "En Proceso",
                OrderStatus.Shipped => "Enviado",
                OrderStatus.Delivered => "Entregado",
                OrderStatus.Cancelled => "Cancelado",
                _ => "Desconocido"
            };
        }

        private string GetPaymentMethodText(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Efectivo => "Efectivo contra entrega",
                PaymentMethod.SINPE => "SINPE Móvil",
                _ => "Desconocido"
            };
        }
    }
}