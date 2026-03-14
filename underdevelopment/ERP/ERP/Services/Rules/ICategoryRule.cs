using ERP.Models;

namespace ERP.Services.Rules
{
    public interface ICategoryRule
    {
        // Megmondja, hogy kötelező-e megadni a lejárati dátumot
        bool IsExpirationDateRequired();

        // Vegyszereknél
        bool IsSafetyDocumentRequired();

        // Pl hűtés
        bool IsSpecialStorageRequired();

        // Ez végzi a validációt (ha valami nem szabályos, dobhatunk Exception-t)
        void ValidateProduct(Product product);
    }
}
