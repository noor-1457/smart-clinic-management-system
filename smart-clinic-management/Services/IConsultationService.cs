using smart_clinic_management.DTOs;

namespace smart_clinic_management.Services;

public interface IConsultationService
{
    Task<ConsultationResponseDto> CreateAsync(ConsultationCreateDto dto);
}

