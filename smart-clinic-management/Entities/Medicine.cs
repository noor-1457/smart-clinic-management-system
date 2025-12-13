using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.Entities;

public class Medicine
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int Quantity { get; set; }

    public int MinimumThreshold { get; set; }

    [MaxLength(40)]
    public string? Unit { get; set; }

    public decimal PricePerUnit { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}

