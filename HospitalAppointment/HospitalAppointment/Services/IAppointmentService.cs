using HospitalAppointment.Models;

namespace HospitalAppointment.Services
{
    public interface IAppointmentService
    {
        Task<bool> TryBookAppointmentAsync(string patientId, string doctorId, DateTime startTime, DateTime endTime);
    }
}