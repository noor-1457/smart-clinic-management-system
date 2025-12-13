using Microsoft.AspNetCore.Mvc;
using smart_clinic_management.DTOs;
using smart_clinic_management.Services;

namespace smart_clinic_management.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;

    public PrescriptionsController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PrescriptionCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _prescriptionService.CreateAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

