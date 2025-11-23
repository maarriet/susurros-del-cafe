// Models/Customer.cs
using System.ComponentModel.DataAnnotations;

namespace Susurros_del_Cafe_WEB.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; }

        [Required]
        [StringLength(50)]
        public string Province { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        // Navigation property
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}