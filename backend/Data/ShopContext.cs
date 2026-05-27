using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CloudNativeShop.Backend.Models;

namespace backend.Data
{
    public class ShopContext : DbContext
    {
        public ShopContext(DbContextOptions<ShopContext> options)
            : base(options)
        {
        }

        public DbSet<CloudNativeShop.Backend.Models.Product> Product { get; set; } = default!;
        public DbSet<CloudNativeShop.Backend.Models.Category> Category { get; set; } = default!;
        public DbSet<CloudNativeShop.Backend.Models.Customer> Customer { get; set; } = default!;
        public DbSet<CloudNativeShop.Backend.Models.Seller> Seller { get; set; } = default!;
        public DbSet<CloudNativeShop.Backend.Models.Order> Order { get; set; } = default!;
        public DbSet<CloudNativeShop.Backend.Models.OrderItem> OrderItem { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CloudNativeShop.Backend.Models.OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<CloudNativeShop.Backend.Models.OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.GetProperties()
                        .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));
                    foreach (var property in properties)
                    {
                        property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<decimal, double>(
                            v => (double)v,
                            v => (decimal)v));
                    }
                }
            }
        }
    }
}
