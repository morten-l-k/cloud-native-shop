using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class PriceService(ShopContext context)
    {
        public async Task<bool> SetPriceAsync(string productId, string sellerId, decimal newPrice)
        {
            int rows = await context.Product
                .Where(p => p.ProductId == productId && p.SellerId == sellerId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ProductPrice, newPrice));
            return rows > 0;
        }
    }
}
