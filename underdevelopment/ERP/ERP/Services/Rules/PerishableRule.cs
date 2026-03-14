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
            if (product.ExpirationDate == null)
            {
                throw new Exception("Lejárati dátum megadása kötelező a romlandó termékeknél.");
            }

        }
    }
}
