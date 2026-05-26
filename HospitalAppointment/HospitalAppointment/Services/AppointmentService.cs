using HospitalAppointment.Data;
using HospitalAppointment.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> TryBookAppointmentAsync(string patientId, string doctorId, DateTime startTime, DateTime endTime)
        {
            // Rendel-e
            var dayOfWeek = startTime.DayOfWeek;
            var timeOfDayStart = startTime.TimeOfDay;
            var timeOfDayEnd = endTime.TimeOfDay;

            bool isDoctorWorking = await _context.DoctorSchedules
                .AnyAsync(s => s.DoctorId == doctorId &&
                               s.Day == dayOfWeek &&
                               s.StartTime <= timeOfDayStart &&
                               s.EndTime >= timeOfDayEnd);

            if (!isDoctorWorking)
            {
                return false;
            }

            // Ütközés ellenőrzése
            bool hasConflict = await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId &&
                               a.Status != AppointmentStatus.Cancelled && 
                               a.StartTime < endTime &&
                               a.EndTime > startTime);

            if (hasConflict)
            {
                return false;
            }



            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                StartTime = startTime,
                EndTime = endTime,
                Status = AppointmentStatus.Pending 
            };

            try
            {
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync(); 
                return true; 
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
    }
}