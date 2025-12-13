using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.DTOs;

public class ConsultationCreateDto
{
    [Required]
    public Guid AppointmentId { get; set; }

    [MaxLength(500)]
    public string? Diagnosis { get; set; }

    [MaxLength(1000)]
    public string? Observations { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? TestRecommendations { get; set; }
}

public class ConsultationResponseDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string? Diagnosis { get; set; }
    public string? Observations { get; set; }
    public string? Notes { get; set; }
    public string? TestRecommendations { get; set; }
    public DateTime CreatedAt { get; set; }
}

