using smart_clinic_management.DTOs;

namespace smart_clinic_management.Services;

public interface IInvoiceService
{
    Task<InvoiceResponseDto> CreateAsync(InvoiceCreateDto dto);
    Task<InvoiceResponseDto> MarkPaidAsync(Guid invoiceId);
    Task<InvoiceResponseDto> GetAsync(Guid invoiceId);
    Task<Stream> GetPdfAsync(Guid invoiceId);
}

