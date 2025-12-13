using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.Entities;

public class Prescription
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid AppointmentId { get; set; }

    public Appointment? Appointment { get; set; }

    [Required]
    public Guid DoctorId { get; set; }

    public Doctor? Doctor { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    public Patient? Patient { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}

