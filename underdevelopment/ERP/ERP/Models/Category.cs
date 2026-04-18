using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.Models
{
    public enum CategoryType
    {
        Standard,
        Perishable,  //romlandó áru (kell lejárati idő)
		Hazardous,   //veszélyes áru (pl. vegyi anyagok, robbanóanyagok)
		LivingOrganism, //élő szervezet (pl. növények, állatok)
	};

    public class Category
    {
		public int Id { get; set; }

        [Required]
		[StringLength(100)]
		public required string Name{ get; set; }
		public CategoryType Type{ get; set; }
        public virtual ICollection<Product> Products{ get; set; } = new List<Product>();

    }
}
