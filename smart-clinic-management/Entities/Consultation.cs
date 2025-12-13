using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.Entities;

public class Consultation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid AppointmentId { get; set; }

    public Appointment? Appointment { get; set; }

    [MaxLength(500)]
    public string? Diagnosis { get; set; }

    [MaxLength(1000)]
    public string? Observations { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? TestRecommendations { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

