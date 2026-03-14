using ERP.Models;

namespace ERP.Services.Rules
{
    public class StandardRule : ICategoryRule
    {
        // A sima termékeknél nem szükségesek
        public bool IsExpirationDateRequired() => false;
        public bool IsSafetyDocumentRequired() => false;
        public bool IsSpecialStorageRequired() => false;

        public void ValidateProduct(Product product) 
        {
            // Nincs extra validáció
        }
    }
}
