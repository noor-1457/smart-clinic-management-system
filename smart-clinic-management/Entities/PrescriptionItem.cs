using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.Entities;

public class PrescriptionItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid PrescriptionId { get; set; }

    public Prescription? Prescription { get; set; }

    [Required]
    public Guid MedicineId { get; set; }

    public Medicine? Medicine { get; set; }

    [MaxLength(200)]
    public string? Dosage { get; set; }

    public int Quantity { get; set; }

    [MaxLength(300)]
    public string? Instructions { get; set; }
}

