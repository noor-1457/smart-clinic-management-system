using System.ComponentModel.DataAnnotations;
using smart_clinic_management.Entities;

namespace smart_clinic_management.DTOs;

public class InvoiceItemDto
{
    [Required, MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}

public class InvoiceCreateDto
{
    [Required]
    public Guid AppointmentId { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    [MinLength(1)]
    public List<InvoiceItemDto> Items { get; set; } = new();
}

public class InvoiceResponseDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}

