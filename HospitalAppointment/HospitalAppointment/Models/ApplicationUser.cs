using Microsoft.AspNetCore.Identity;

namespace HospitalAppointment.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string ? FullName { get; set; }
        public string? Specialty { get; set; } 
    }

    public static class Specialties
    {
        public static readonly List<string> List = new()
        {
            "Sebész", "Fül-orr gégész", "Nőgyógyász", "Házi orvos", "Neurológus", "Szemész"
        };
    }
}
