// Services/IEmailService.cs
using Susurros_del_Cafe_WEB.Models;

namespace Susurros_del_Cafe_WEB.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(Order order);
    }
}