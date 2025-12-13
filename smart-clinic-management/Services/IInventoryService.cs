using smart_clinic_management.DTOs;

namespace smart_clinic_management.Services;

public interface IInventoryService
{
    Task<MedicineResponseDto> CreateAsync(MedicineCreateDto dto);
    Task<MedicineResponseDto> UpdateAsync(Guid id, MedicineUpdateDto dto);
    Task DeleteAsync(Guid id);
    Task<List<MedicineResponseDto>> GetAllAsync();
    Task<MedicineResponseDto> GetByIdAsync(Guid id);
    Task<List<MedicineResponseDto>> GetLowStockAsync();
    Task DeductStockAsync(Guid medicineId, int quantity);
}

