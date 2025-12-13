using Microsoft.EntityFrameworkCore;
using smart_clinic_management.DTOs;
using smart_clinic_management.Entities;
using smart_clinic_management.Repositories;

namespace smart_clinic_management.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly IRepository<Doctor> _doctorRepository;
    private readonly IRepository<Patient> _patientRepository;

    public AppointmentService(
        IRepository<Appointment> appointmentRepository,
        IRepository<Doctor> doctorRepository,
        IRepository<Patient> patientRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
    }

    public async Task<AppointmentResponseDto> CreateAsync(AppointmentCreateDto dto)
    {
        var doctor = await _doctorRepository.GetByIdAsync(dto.DoctorId) ?? throw new InvalidOperationException("Doctor not found.");
        var patient = await _patientRepository.GetByIdAsync(dto.PatientId) ?? throw new InvalidOperationException("Patient not found.");

        if (dto.ScheduledAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Appointment time must be in the future.");
        }

        var hasConflict = await _appointmentRepository.Query()
            .AnyAsync(a => a.DoctorId == dto.DoctorId &&
                           a.ScheduledAt == dto.ScheduledAt &&
                           a.Status != AppointmentStatus.Rejected);

        if (hasConflict)
        {
            throw new InvalidOperationException("Doctor already has an appointment at this time.");
        }

        var appointment = new Appointment
        {
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            ScheduledAt = dto.ScheduledAt,
            Reason = dto.Reason,
            Status = AppointmentStatus.Pending
        };

        await _appointmentRepository.AddAsync(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return Map(appointment);
    }

    public async Task<AppointmentResponseDto> UpdateStatusAsync(Guid appointmentId, AppointmentStatus status)
    {
        if (status is not (AppointmentStatus.Approved or AppointmentStatus.Rejected or AppointmentStatus.Completed))
        {
            throw new InvalidOperationException("Unsupported status transition.");
        }

        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId) ?? throw new InvalidOperationException("Appointment not found.");

        if (appointment.Status == AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Completed appointments cannot be updated.");
        }

        appointment.Status = status;

        if (status == AppointmentStatus.Completed)
        {
            appointment.CompletedAt = DateTime.UtcNow;
        }

        _appointmentRepository.Update(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return Map(appointment);
    }

    public async Task<List<AppointmentResponseDto>> GetDoctorAppointmentsAsync(Guid doctorId)
    {
        var doctor = await _doctorRepository.GetByIdAsync(doctorId) ?? throw new InvalidOperationException("Doctor not found.");

        var appointments = await _appointmentRepository.Query()
            .Where(a => a.DoctorId == doctor.Id)
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync();

        return appointments.Select(Map).ToList();
    }

    public async Task<List<AppointmentResponseDto>> GetPatientAppointmentsAsync(Guid patientId)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId) ?? throw new InvalidOperationException("Patient not found.");

        var appointments = await _appointmentRepository.Query()
            .Where(a => a.PatientId == patient.Id)
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync();

        return appointments.Select(Map).ToList();
    }

    private static AppointmentResponseDto Map(Appointment appointment) => new()
    {
        Id = appointment.Id,
        DoctorId = appointment.DoctorId,
        PatientId = appointment.PatientId,
        ScheduledAt = appointment.ScheduledAt,
        Reason = appointment.Reason,
        Status = appointment.Status,
        CreatedAt = appointment.CreatedAt
    };
}

