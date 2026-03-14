using ERP.Models;

namespace ERP.Services.Rules
{
    public class HazardousRule : ICategoryRule
    {
        public bool IsExpirationDateRequired() => false;
        public bool IsSpecialStorageRequired() => false;
        public bool IsSafetyDocumentRequired() => true;
        public void ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.SafetyDocumentURL))
            {
                throw new InvalidOperationException("Veszélyes áruhoz kötelező biztonsági adatlap csatolása");
            }
        }
    }
}
