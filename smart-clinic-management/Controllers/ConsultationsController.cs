using Microsoft.AspNetCore.Mvc;
using smart_clinic_management.DTOs;
using smart_clinic_management.Services;

namespace smart_clinic_management.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsultationsController : ControllerBase
{
    private readonly IConsultationService _consultationService;

    public ConsultationsController(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ConsultationCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _consultationService.CreateAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

