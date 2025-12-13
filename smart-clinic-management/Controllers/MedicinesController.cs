using Microsoft.AspNetCore.Mvc;
using smart_clinic_management.DTOs;
using smart_clinic_management.Services;

namespace smart_clinic_management.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicinesController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public MedicinesController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _inventoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _inventoryService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var result = await _inventoryService.GetLowStockAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MedicineCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _inventoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] MedicineUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _inventoryService.UpdateAsync(id, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _inventoryService.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

