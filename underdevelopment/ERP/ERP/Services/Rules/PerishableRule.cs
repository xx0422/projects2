using ERP.Models;

namespace ERP.Services.Rules
{
    public class PerishableRule : ICategoryRule
    {
        public bool IsExpirationDateRequired() => true;
        public bool IsSpecialStorageRequired() => true;
        public bool IsSafetyDocumentRequired() => false;
        public void ValidateProduct(Product product)
        {
            product.IsPerishable = true;

        }
    }
}
