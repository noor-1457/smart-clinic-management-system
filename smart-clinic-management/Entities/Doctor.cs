using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.Entities;

public class Doctor
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Specialization { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}

