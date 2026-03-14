using ERP.Models;

namespace ERP.Services.Rules
{
    public class LivingOrganismRule : ICategoryRule
    {
        public bool IsExpirationDateRequired() => true;
        public bool IsSpecialStorageRequired() => true;
        public bool IsSafetyDocumentRequired() => false;
        public void ValidateProduct(Product product)
        {
            if (string.IsNullOrEmpty(product.FoodSafetyCertificateId))
            {
                throw new Exception("Hiba: Élő élelmiszerhez kötelező az érvényes élelmiszer-biztonsági igazolás!");
            }
        }
    }
}
