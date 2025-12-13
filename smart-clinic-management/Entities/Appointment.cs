using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.Entities;

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid DoctorId { get; set; }

    public Doctor? Doctor { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    public Patient? Patient { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

