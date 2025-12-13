using Microsoft.EntityFrameworkCore;
using smart_clinic_management.DTOs;
using smart_clinic_management.Entities;
using smart_clinic_management.Repositories;

namespace smart_clinic_management.Services;

public class ConsultationService : IConsultationService
{
    private readonly IRepository<Consultation> _consultationRepository;
    private readonly IRepository<Appointment> _appointmentRepository;

    public ConsultationService(IRepository<Consultation> consultationRepository, IRepository<Appointment> appointmentRepository)
    {
        _consultationRepository = consultationRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<ConsultationResponseDto> CreateAsync(ConsultationCreateDto dto)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId) ?? throw new InvalidOperationException("Appointment not found.");

        if (appointment.Status is AppointmentStatus.Pending or AppointmentStatus.Rejected)
        {
            throw new InvalidOperationException("Consultation can only be added to approved or completed appointments.");
        }

        var alreadyExists = await _consultationRepository.Query()
            .AnyAsync(c => c.AppointmentId == appointment.Id);

        if (alreadyExists)
        {
            throw new InvalidOperationException("Consultation already exists for this appointment.");
        }

        var consultation = new Consultation
        {
            AppointmentId = appointment.Id,
            Diagnosis = dto.Diagnosis,
            Observations = dto.Observations,
            Notes = dto.Notes,
            TestRecommendations = dto.TestRecommendations
        };

        await _consultationRepository.AddAsync(consultation);
        await _consultationRepository.SaveChangesAsync();

        return Map(consultation);
    }

    private static ConsultationResponseDto Map(Consultation consultation) => new()
    {
        Id = consultation.Id,
        AppointmentId = consultation.AppointmentId,
        Diagnosis = consultation.Diagnosis,
        Observations = consultation.Observations,
        Notes = consultation.Notes,
        TestRecommendations = consultation.TestRecommendations,
        CreatedAt = consultation.CreatedAt
    };
}

