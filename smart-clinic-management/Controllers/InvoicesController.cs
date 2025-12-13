using Microsoft.AspNetCore.Mvc;
using smart_clinic_management.DTOs;
using smart_clinic_management.Services;

namespace smart_clinic_management.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InvoiceCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _invoiceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { invoiceId = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{invoiceId:guid}")]
    public async Task<IActionResult> GetById(Guid invoiceId)
    {
        try
        {
            var result = await _invoiceService.GetAsync(invoiceId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{invoiceId:guid}/pay")]
    public async Task<IActionResult> MarkPaid(Guid invoiceId)
    {
        try
        {
            var result = await _invoiceService.MarkPaidAsync(invoiceId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{invoiceId:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid invoiceId)
    {
        try
        {
            var stream = await _invoiceService.GetPdfAsync(invoiceId);
            return File(stream, "application/pdf", $"invoice-{invoiceId}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

