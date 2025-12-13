using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.Entities;

public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public DateTime DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

