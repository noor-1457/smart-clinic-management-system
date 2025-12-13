using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.Entities;

public class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid AppointmentId { get; set; }

    public Appointment? Appointment { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    public Patient? Patient { get; set; }

    public decimal TotalAmount { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    public byte[]? PdfData { get; set; }

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}

