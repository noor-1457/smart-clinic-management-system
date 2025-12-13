using smart_clinic_management.DTOs;

namespace smart_clinic_management.Services;

public interface IPrescriptionService
{
    Task<PrescriptionResponseDto> CreateAsync(PrescriptionCreateDto dto);
}

