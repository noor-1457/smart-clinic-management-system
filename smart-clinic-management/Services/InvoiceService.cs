using System.Text;
using Microsoft.EntityFrameworkCore;
using smart_clinic_management.Data;
using smart_clinic_management.DTOs;
using smart_clinic_management.Entities;
using smart_clinic_management.Repositories;

namespace smart_clinic_management.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ClinicDbContext _context;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly IRepository<Patient> _patientRepository;

    public InvoiceService(
        ClinicDbContext context,
        IRepository<Invoice> invoiceRepository,
        IRepository<Appointment> appointmentRepository,
        IRepository<Patient> patientRepository)
    {
        _context = context;
        _invoiceRepository = invoiceRepository;
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
    }

    public async Task<InvoiceResponseDto> CreateAsync(InvoiceCreateDto dto)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId) ?? throw new InvalidOperationException("Appointment not found.");
        var patient = await _patientRepository.GetByIdAsync(dto.PatientId) ?? throw new InvalidOperationException("Patient not found.");

        if (appointment.PatientId != patient.Id)
        {
            throw new InvalidOperationException("Appointment and patient mismatch.");
        }

        var invoice = new Invoice
        {
            AppointmentId = appointment.Id,
            PatientId = patient.Id,
            Status = InvoiceStatus.Unpaid
        };

        await _invoiceRepository.AddAsync(invoice);
        await _invoiceRepository.SaveChangesAsync();

        var items = dto.Items.Select(i => new InvoiceItem
        {
            InvoiceId = invoice.Id,
            Description = i.Description,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList();

        await _context.InvoiceItems.AddRangeAsync(items);

        invoice.Items = items;
        invoice.TotalAmount = items.Sum(i => i.LineTotal);
        invoice.PdfData = GeneratePdf(invoice, appointment, patient);

        _invoiceRepository.Update(invoice);
        await _invoiceRepository.SaveChangesAsync();

        return Map(invoice);
    }

    public async Task<InvoiceResponseDto> MarkPaidAsync(Guid invoiceId)
    {
        var invoice = await _invoiceRepository.Query()
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == invoiceId) ?? throw new InvalidOperationException("Invoice not found.");

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAt = DateTime.UtcNow;
        _invoiceRepository.Update(invoice);
        await _invoiceRepository.SaveChangesAsync();

        return Map(invoice);
    }

    public async Task<InvoiceResponseDto> GetAsync(Guid invoiceId)
    {
        var invoice = await _invoiceRepository.Query()
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == invoiceId) ?? throw new InvalidOperationException("Invoice not found.");

        return Map(invoice);
    }

    public async Task<Stream> GetPdfAsync(Guid invoiceId)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId) ?? throw new InvalidOperationException("Invoice not found.");

        if (invoice.PdfData == null || invoice.PdfData.Length == 0)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(invoice.AppointmentId) ?? throw new InvalidOperationException("Appointment not found.");
            var patient = await _patientRepository.GetByIdAsync(invoice.PatientId) ?? throw new InvalidOperationException("Patient not found.");
            invoice.PdfData = GeneratePdf(invoice, appointment, patient);
            _invoiceRepository.Update(invoice);
            await _invoiceRepository.SaveChangesAsync();
        }

        return new MemoryStream(invoice.PdfData);
    }

    private static byte[] GeneratePdf(Invoice invoice, Appointment appointment, Patient patient)
    {
        // Minimal PDF-like content; replace with proper PDF generator in production
        var builder = new StringBuilder();
        builder.AppendLine("Smart Clinic Invoice");
        builder.AppendLine($"Invoice Id: {invoice.Id}");
        builder.AppendLine($"Appointment Id: {appointment.Id}");
        builder.AppendLine($"Patient: {patient.FullName}");
        builder.AppendLine($"Status: {invoice.Status}");
        builder.AppendLine($"Created: {invoice.CreatedAt:O}");
        builder.AppendLine($"Total: {invoice.TotalAmount:C}");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static InvoiceResponseDto Map(Invoice invoice)
    {
        var items = invoice.Items ?? Enumerable.Empty<InvoiceItem>();
        return new InvoiceResponseDto
        {
            Id = invoice.Id,
            AppointmentId = invoice.AppointmentId,
            PatientId = invoice.PatientId,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            Items = items.Select(i => new InvoiceItemDto
            {
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}

