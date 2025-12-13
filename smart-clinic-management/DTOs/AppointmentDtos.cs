using System.ComponentModel.DataAnnotations;
using smart_clinic_management.Entities;

namespace smart_clinic_management.DTOs;

public class AppointmentCreateDto
{
    [Required]
    public Guid DoctorId { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}

public class AppointmentStatusUpdateDto
{
    [Required]
    public AppointmentStatus Status { get; set; }
}

public class AppointmentResponseDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? Reason { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

