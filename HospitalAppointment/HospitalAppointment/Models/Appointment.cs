using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Models
{
    public enum AppointmentStatus { Pending, Confirmed, Completed, Cancelled }
    public class Appointment
    {
        public int Id { get; set; }
        [Required]
        public string ? DoctorId { get; set; }
        public ApplicationUser ? Doctor { get; set; }

        [Required]
        public string ? PatientId { get; set; }
        public ApplicationUser ? Patient { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    }
}
