using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.DTOs;

public class PrescriptionItemCreateDto
{
    [Required]
    public Guid MedicineId { get; set; }

    [MaxLength(200)]
    public string? Dosage { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [MaxLength(300)]
    public string? Instructions { get; set; }
}

public class PrescriptionCreateDto
{
    [Required]
    public Guid AppointmentId { get; set; }

    [Required]
    public Guid DoctorId { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    [MinLength(1, ErrorMessage = "At least one prescription item is required.")]
    public List<PrescriptionItemCreateDto> Items { get; set; } = new();
}

public class PrescriptionItemResponseDto
{
    public Guid MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Dosage { get; set; }
    public string? Instructions { get; set; }
}

public class PrescriptionResponseDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PrescriptionItemResponseDto> Items { get; set; } = new();
}

