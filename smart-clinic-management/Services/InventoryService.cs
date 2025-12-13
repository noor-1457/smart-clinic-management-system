using Microsoft.EntityFrameworkCore;
using smart_clinic_management.DTOs;
using smart_clinic_management.Entities;
using smart_clinic_management.Repositories;

namespace smart_clinic_management.Services;

public class InventoryService : IInventoryService
{
    private readonly IRepository<Medicine> _medicineRepository;
    private readonly ILowStockAlertService _lowStockAlertService;

    public InventoryService(IRepository<Medicine> medicineRepository, ILowStockAlertService lowStockAlertService)
    {
        _medicineRepository = medicineRepository;
        _lowStockAlertService = lowStockAlertService;
    }

    public async Task<MedicineResponseDto> CreateAsync(MedicineCreateDto dto)
    {
        var exists = await _medicineRepository.Query().AnyAsync(m => m.Name == dto.Name);
        if (exists)
        {
            throw new InvalidOperationException("Medicine with the same name already exists.");
        }

        var medicine = new Medicine
        {
            Name = dto.Name,
            Description = dto.Description,
            Quantity = dto.Quantity,
            MinimumThreshold = dto.MinimumThreshold,
            Unit = dto.Unit,
            PricePerUnit = dto.PricePerUnit
        };

        await _medicineRepository.AddAsync(medicine);
        await _medicineRepository.SaveChangesAsync();
        await CheckLowStockAsync(medicine);

        return Map(medicine);
    }

    public async Task<MedicineResponseDto> UpdateAsync(Guid id, MedicineUpdateDto dto)
    {
        var medicine = await _medicineRepository.GetByIdAsync(id) ?? throw new InvalidOperationException("Medicine not found.");

        medicine.Name = dto.Name;
        medicine.Description = dto.Description;
        medicine.Quantity = dto.Quantity;
        medicine.MinimumThreshold = dto.MinimumThreshold;
        medicine.Unit = dto.Unit;
        medicine.PricePerUnit = dto.PricePerUnit;
        medicine.IsActive = dto.IsActive;

        _medicineRepository.Update(medicine);
        await _medicineRepository.SaveChangesAsync();
        await CheckLowStockAsync(medicine);

        return Map(medicine);
    }

    public async Task DeleteAsync(Guid id)
    {
        var medicine = await _medicineRepository.GetByIdAsync(id) ?? throw new InvalidOperationException("Medicine not found.");
        _medicineRepository.Remove(medicine);
        await _medicineRepository.SaveChangesAsync();
    }

    public async Task<List<MedicineResponseDto>> GetAllAsync()
    {
        var medicines = await _medicineRepository.Query().OrderBy(m => m.Name).ToListAsync();
        return medicines.Select(Map).ToList();
    }

    public async Task<MedicineResponseDto> GetByIdAsync(Guid id)
    {
        var medicine = await _medicineRepository.GetByIdAsync(id) ?? throw new InvalidOperationException("Medicine not found.");
        return Map(medicine);
    }

    public async Task<List<MedicineResponseDto>> GetLowStockAsync()
    {
        var medicines = await _medicineRepository.Query()
            .Where(m => m.Quantity <= m.MinimumThreshold)
            .ToListAsync();

        return medicines.Select(Map).ToList();
    }

    public async Task DeductStockAsync(Guid medicineId, int quantity)
    {
        var medicine = await _medicineRepository.GetByIdAsync(medicineId) ?? throw new InvalidOperationException("Medicine not found.");

        if (quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        if (medicine.Quantity < quantity)
        {
            throw new InvalidOperationException($"Insufficient stock for {medicine.Name}. Available: {medicine.Quantity}.");
        }

        medicine.Quantity -= quantity;
        _medicineRepository.Update(medicine);
        await _medicineRepository.SaveChangesAsync();
        await CheckLowStockAsync(medicine);
    }

    private async Task CheckLowStockAsync(Medicine medicine)
    {
        if (medicine.Quantity <= medicine.MinimumThreshold)
        {
            await _lowStockAlertService.HandleLowStockAsync(medicine.Name, medicine.Quantity, medicine.MinimumThreshold);
        }
    }

    private static MedicineResponseDto Map(Medicine medicine) => new()
    {
        Id = medicine.Id,
        Name = medicine.Name,
        Description = medicine.Description,
        Quantity = medicine.Quantity,
        MinimumThreshold = medicine.MinimumThreshold,
        Unit = medicine.Unit,
        PricePerUnit = medicine.PricePerUnit,
        IsActive = medicine.IsActive
    };
}

