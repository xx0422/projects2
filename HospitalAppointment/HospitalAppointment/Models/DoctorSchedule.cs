using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Models
{
    public class DoctorSchedule
    {
        public int Id { get; set; }

        [Required]
        public string ? DoctorId { get; set; }
        public ApplicationUser ? Doctor { get; set; } // Kapcsolat a felhasználóhoz

        public DayOfWeek Day { get; set; } 
        public TimeSpan StartTime { get; set; } 
        public TimeSpan EndTime { get; set; }  
    }
}
