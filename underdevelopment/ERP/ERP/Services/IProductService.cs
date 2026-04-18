using ERP.Models;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product?>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);

        Task CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);

        Task ProcessStockReceiptAsync(int productId, int warehouseId, decimal quantity, decimal unitPrice);
        Task TransferStockAsync(int productId, int fromWarehouseId, int toWarehouseId, decimal quantity);
        Task? GetProductsByCategory(int categoryId);
        Task ProcessStockIssueAsync(int productId, int warehouseId, decimal quantity);
        Task<IEnumerable<object>> GetStockByWarehouseAsync(int warehouseId);
        Task<IEnumerable<object>> GetInventorySummaryAsync();
        Task<IEnumerable<object>> GetProductLocationsAsync(int productId, int quantity);
    }
}
