using smart_clinic_management.DTOs;
using smart_clinic_management.Entities;

namespace smart_clinic_management.Services;

public interface IAppointmentService
{
    Task<AppointmentResponseDto> CreateAsync(AppointmentCreateDto dto);
    Task<AppointmentResponseDto> UpdateStatusAsync(Guid appointmentId, AppointmentStatus status);
    Task<List<AppointmentResponseDto>> GetDoctorAppointmentsAsync(Guid doctorId);
    Task<List<AppointmentResponseDto>> GetPatientAppointmentsAsync(Guid patientId);
}

