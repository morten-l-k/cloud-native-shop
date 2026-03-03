using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CloudNativeShop.Backend.Models;

namespace backend.Data
{
    public class Product : DbContext
    {
        public Product (DbContextOptions<Product> options)
            : base(options)
        {
        }

        public DbSet<CloudNativeShop.Backend.Models.ProductViewModel> ProductViewModel { get; set; } = default!;
    }
}
