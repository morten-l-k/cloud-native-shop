using backend.Data;
using CloudNativeShop.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class StockService(ShopContext context)
    {
        public async Task<int> GetStockAsync(string productId) =>
            await context.Product
                .Where(p => p.ProductId == productId)
                .Select(p => p.ProductStock ?? 0)
                .FirstOrDefaultAsync();

        public async Task<bool> CheckAndDecrementAsync(string productId, int quantity)
        {
            int rows = await context.Product
                .Where(p => p.ProductId == productId && p.ProductStock >= quantity)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(p => p.ProductStock, p => p.ProductStock - quantity));
            return rows > 0;
        }

        public async Task<bool> SetStockAsync(string productId, string sellerId, int newStock)
        {
            int rows = await context.Product
                .Where(p => p.ProductId == productId && p.SellerId == sellerId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ProductStock, newStock));
            return rows > 0;
        }
    }
}
