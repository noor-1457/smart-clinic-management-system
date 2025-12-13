using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.DTOs;

public class MedicineCreateDto
{
    [Required, MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumThreshold { get; set; }

    [MaxLength(40)]
    public string? Unit { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PricePerUnit { get; set; }
}

public class MedicineUpdateDto : MedicineCreateDto
{
    public bool IsActive { get; set; } = true;
}

public class MedicineResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public int MinimumThreshold { get; set; }
    public string? Unit { get; set; }
    public decimal PricePerUnit { get; set; }
    public bool IsActive { get; set; }
}

