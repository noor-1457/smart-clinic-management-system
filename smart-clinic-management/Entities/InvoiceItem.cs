using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.Entities;

public class InvoiceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid InvoiceId { get; set; }

    public Invoice? Invoice { get; set; }

    [Required, MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}

