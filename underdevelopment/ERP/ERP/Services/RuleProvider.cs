using ERP.Models;
using ERP.Services.Rules;

namespace ERP.Services
{
    public static class RuleProvider
    {
        public static List<ICategoryRule> GetRulesForProduct(Product product)
        {
            var rules = new List<ICategoryRule>();

            // 1. Minden termékre vonatkozik az alap szabály
            rules.Add(new StandardRule());

            
            if (product.Category != null)
            {
                switch (product.Category.Type) 
                {
                    case CategoryType.Perishable:
                        rules.Add(new PerishableRule());
                        break;
                    case CategoryType.Hazardous:
                        rules.Add(new HazardousRule());
                        break;
                    case CategoryType.LivingOrganism:
                        rules.Add(new LivingOrganismRule());
                        break;
                }
            }

            return rules;
        }
    }
}
