using Microsoft.AspNetCore.Mvc;
using smart_clinic_management.DTOs;
using smart_clinic_management.Entities;
using smart_clinic_management.Services;

namespace smart_clinic_management.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _appointmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByPatient), new { patientId = result.PatientId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{appointmentId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid appointmentId, [FromBody] AppointmentStatusUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _appointmentService.UpdateStatusAsync(appointmentId, dto.Status);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("doctor/{doctorId:guid}")]
    public async Task<IActionResult> GetByDoctor(Guid doctorId)
    {
        try
        {
            var result = await _appointmentService.GetDoctorAppointmentsAsync(doctorId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> GetByPatient(Guid patientId)
    {
        try
        {
            var result = await _appointmentService.GetPatientAppointmentsAsync(patientId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

