// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Susurros_del_Cafe_WEB.Models;

namespace Susurros_del_Cafe_WEB.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId);

            // Seed data
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Café Susurros 250g",
                    Description = "Café artesanal de alta calidad, tueste medio",
                    Price = 3500,
                    Stock = 100,
                    IsActive = true
                },
                new Product
                {
                    Id = 2,
                    Name = "Café Susurros 500g",
                    Description = "Café artesanal de alta calidad, tueste medio",
                    Price = 6500,
                    Stock = 100,
                    IsActive = true
                }
            );
        }
    }
}