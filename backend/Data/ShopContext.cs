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
    }
}
