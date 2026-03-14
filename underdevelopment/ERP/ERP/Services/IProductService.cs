using ERP.Models;

namespace ERP.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product?>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);

        Task CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);

        Task ProcessStockReceiptAsync(int productId, decimal quantity, decimal unitPrice);
        Task? GetProductsByCategory(int categoryId);
        Task ProcessStockIssueAsync(int productId, decimal quantity);
        Task<IEnumerable<object>> GetInventorySummaryAsync();
    }
}
