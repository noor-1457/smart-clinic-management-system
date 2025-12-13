using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smart_clinic_management.Data;
using smart_clinic_management.Services;

namespace smart_clinic_management.Controllers;

/// <summary>
/// Controller to handle UI section clicks and return data for frontend display
/// This controller provides backend functionality for UI interactions
/// </summary>
[ApiController]
[Route("api/ui")]
public class UIController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IInventoryService _inventoryService;
    private readonly IInvoiceService _invoiceService;
    private readonly ILowStockAlertService _lowStockAlertService;
    private readonly ClinicDbContext _context;

    public UIController(
        IAppointmentService appointmentService,
        IInventoryService inventoryService,
        IInvoiceService invoiceService,
        ILowStockAlertService lowStockAlertService,
        ClinicDbContext context)
    {
        _appointmentService = appointmentService;
        _inventoryService = inventoryService;
        _invoiceService = invoiceService;
        _lowStockAlertService = lowStockAlertService;
        _context = context;
    }

    /// <summary>
    /// Get data for a specific UI section when clicked
    /// Usage: GET /api/ui/section/{sectionName}
    /// </summary>
    [HttpGet("section/{sectionName}")]
    public async Task<IActionResult> GetSectionData(string sectionName)
    {
        try
        {
            var sectionData = sectionName.ToLower() switch
            {
                "appointments" => await GetAppointmentsSectionData(),
                "dashboard" => await GetDashboardSectionData(),
                "inventory" => await GetInventorySectionData(),
                "medicines" => await GetMedicinesSectionData(),
                "lowstock" => await GetLowStockSectionData(),
                "invoices" => await GetInvoicesSectionData(),
                "statistics" => await GetStatisticsSectionData(),
                "patients" => await GetPatientsSectionData(),
                "doctors" => await GetDoctorsSectionData(),
                _ => null
            };

            if (sectionData == null)
            {
                return NotFound(new { message = $"Section '{sectionName}' not found" });
            }

            return Ok(sectionData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error loading section data", error = ex.Message });
        }
    }

    /// <summary>
    /// Get appointments section data
    /// </summary>
    private async Task<object> GetAppointmentsSectionData()
    {
        var today = DateTime.UtcNow.Date;
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.ScheduledAt.Date >= today)
            .OrderBy(a => a.ScheduledAt)
            .Take(10)
            .ToListAsync();

        var pendingCount = await _context.Appointments
            .CountAsync(a => a.Status == Entities.AppointmentStatus.Pending);

        var todayCount = await _context.Appointments
            .CountAsync(a => a.ScheduledAt.Date == today);

        return new
        {
            sectionName = "appointments",
            title = "Appointments",
            data = appointments.Select(a => new
            {
                id = a.Id,
                patientName = a.Patient?.FullName,
                doctorName = a.Doctor?.FullName,
                scheduledAt = a.ScheduledAt,
                status = a.Status.ToString(),
                reason = a.Reason
            }),
            summary = new
            {
                totalPending = pendingCount,
                todayAppointments = todayCount,
                upcomingCount = appointments.Count
            }
        };
    }

    /// <summary>
    /// Get dashboard section data with overview statistics
    /// </summary>
    private async Task<object> GetDashboardSectionData()
    {
        var today = DateTime.UtcNow.Date;
        
        var totalAppointments = await _context.Appointments.CountAsync();
        var todayAppointments = await _context.Appointments
            .CountAsync(a => a.ScheduledAt.Date == today);
        var pendingAppointments = await _context.Appointments
            .CountAsync(a => a.Status == Entities.AppointmentStatus.Pending);
        
        var totalPatients = await _context.Patients.CountAsync();
        var totalDoctors = await _context.Doctors.CountAsync();
        
        var totalMedicines = await _context.Medicines.CountAsync();
        var lowStockCount = await _context.Medicines
            .CountAsync(m => m.Quantity <= m.MinimumThreshold);
        
        var totalInvoices = await _context.Invoices.CountAsync();
        var unpaidInvoices = await _context.Invoices
            .CountAsync(i => i.Status == Entities.InvoiceStatus.Unpaid);

        return new
        {
            sectionName = "dashboard",
            title = "Dashboard Overview",
            statistics = new
            {
                appointments = new
                {
                    total = totalAppointments,
                    today = todayAppointments,
                    pending = pendingAppointments
                },
                patients = new
                {
                    total = totalPatients
                },
                doctors = new
                {
                    total = totalDoctors
                },
                inventory = new
                {
                    totalMedicines = totalMedicines,
                    lowStockItems = lowStockCount
                },
                invoices = new
                {
                    total = totalInvoices,
                    unpaid = unpaidInvoices
                }
            }
        };
    }

    /// <summary>
    /// Get inventory section data
    /// </summary>
    private async Task<object> GetInventorySectionData()
    {
        var medicines = await _inventoryService.GetAllAsync();
        var lowStockMedicines = await _inventoryService.GetLowStockAsync();

        return new
        {
            sectionName = "inventory",
            title = "Inventory Management",
            data = medicines,
            lowStockItems = lowStockMedicines,
            summary = new
            {
                totalMedicines = medicines.Count,
                lowStockCount = lowStockMedicines.Count,
                totalItems = medicines.Sum(m => m.Quantity)
            }
        };
    }

    /// <summary>
    /// Get medicines section data
    /// </summary>
    private async Task<object> GetMedicinesSectionData()
    {
        var medicines = await _inventoryService.GetAllAsync();

        return new
        {
            sectionName = "medicines",
            title = "Medicines",
            data = medicines,
            summary = new
            {
                total = medicines.Count,
                active = medicines.Count(m => m.IsActive),
                inactive = medicines.Count(m => !m.IsActive)
            }
        };
    }

    /// <summary>
    /// Get low stock alerts section data
    /// </summary>
    private async Task<object> GetLowStockSectionData()
    {
        var lowStockMedicines = await _inventoryService.GetLowStockAsync();

        return new
        {
            sectionName = "lowstock",
            title = "Low Stock Alerts",
            data = lowStockMedicines,
            summary = new
            {
                lowStockCount = lowStockMedicines.Count,
                criticalItems = lowStockMedicines.Count(m => m.Quantity <= 0)
            }
        };
    }

    /// <summary>
    /// Get invoices section data
    /// </summary>
    private async Task<object> GetInvoicesSectionData()
    {
        var invoices = await _context.Invoices
            .Include(i => i.Patient)
            .OrderByDescending(i => i.CreatedAt)
            .Take(20)
            .ToListAsync();

        var totalAmount = await _context.Invoices
            .Where(i => i.Status == Entities.InvoiceStatus.Paid)
            .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

        var unpaidAmount = await _context.Invoices
            .Where(i => i.Status == Entities.InvoiceStatus.Unpaid)
            .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

        return new
        {
            sectionName = "invoices",
            title = "Invoices",
            data = invoices.Select(i => new
            {
                id = i.Id,
                patientName = i.Patient?.FullName,
                totalAmount = i.TotalAmount,
                status = i.Status.ToString(),
                createdAt = i.CreatedAt,
                paidAt = i.PaidAt
            }),
            summary = new
            {
                totalInvoices = invoices.Count,
                totalPaid = totalAmount,
                totalUnpaid = unpaidAmount,
                paidCount = invoices.Count(i => i.Status == Entities.InvoiceStatus.Paid),
                unpaidCount = invoices.Count(i => i.Status == Entities.InvoiceStatus.Unpaid)
            }
        };
    }

    /// <summary>
    /// Get statistics section data
    /// </summary>
    private async Task<object> GetStatisticsSectionData()
    {
        var today = DateTime.UtcNow.Date;
        var thisMonth = DateTime.UtcNow.Month;
        var thisYear = DateTime.UtcNow.Year;

        var appointmentsThisMonth = await _context.Appointments
            .CountAsync(a => a.ScheduledAt.Month == thisMonth && a.ScheduledAt.Year == thisYear);

        var completedAppointments = await _context.Appointments
            .CountAsync(a => a.Status == Entities.AppointmentStatus.Completed);

        var revenueThisMonth = await _context.Invoices
            .Where(i => i.Status == Entities.InvoiceStatus.Paid && 
                       i.PaidAt.HasValue &&
                       i.PaidAt.Value.Month == thisMonth &&
                       i.PaidAt.Value.Year == thisYear)
            .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

        return new
        {
            sectionName = "statistics",
            title = "Statistics",
            data = new
            {
                appointments = new
                {
                    thisMonth = appointmentsThisMonth,
                    completed = completedAppointments,
                    completionRate = appointmentsThisMonth > 0 
                        ? (double)completedAppointments / appointmentsThisMonth * 100 
                        : 0
                },
                revenue = new
                {
                    thisMonth = revenueThisMonth
                },
                inventory = new
                {
                    totalMedicines = await _context.Medicines.CountAsync(),
                    lowStockCount = await _context.Medicines
                        .CountAsync(m => m.Quantity <= m.MinimumThreshold)
                }
            }
        };
    }

    /// <summary>
    /// Get patients section data
    /// </summary>
    private async Task<object> GetPatientsSectionData()
    {
        var patients = await _context.Patients
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .ToListAsync();

        var totalPatients = await _context.Patients.CountAsync();
        var patientsWithAppointments = await _context.Patients
            .Where(p => p.Appointments.Any())
            .CountAsync();

        return new
        {
            sectionName = "patients",
            title = "Patients",
            data = patients.Select(p => new
            {
                id = p.Id,
                fullName = p.FullName,
                email = p.Email,
                phoneNumber = p.PhoneNumber,
                dateOfBirth = p.DateOfBirth,
                createdAt = p.CreatedAt
            }),
            summary = new
            {
                total = totalPatients,
                withAppointments = patientsWithAppointments,
                recentPatients = patients.Count
            }
        };
    }

    /// <summary>
    /// Get doctors section data
    /// </summary>
    private async Task<object> GetDoctorsSectionData()
    {
        var doctors = await _context.Doctors
            .OrderBy(d => d.FullName)
            .ToListAsync();

        var doctorsWithAppointments = await _context.Doctors
            .Where(d => d.Appointments.Any())
            .CountAsync();

        return new
        {
            sectionName = "doctors",
            title = "Doctors",
            data = doctors.Select(d => new
            {
                id = d.Id,
                fullName = d.FullName,
                specialization = d.Specialization,
                email = d.Email,
                phoneNumber = d.PhoneNumber,
                appointmentCount = d.Appointments.Count
            }),
            summary = new
            {
                total = doctors.Count,
                withAppointments = doctorsWithAppointments
            }
        };
    }

    /// <summary>
    /// Get all available UI sections
    /// Usage: GET /api/ui/sections
    /// </summary>
    [HttpGet("sections")]
    public IActionResult GetAvailableSections()
    {
        var sections = new[]
        {
            new { name = "dashboard", title = "Dashboard", description = "Overview statistics" },
            new { name = "appointments", title = "Appointments", description = "Manage appointments" },
            new { name = "patients", title = "Patients", description = "Patient management" },
            new { name = "doctors", title = "Doctors", description = "Doctor management" },
            new { name = "inventory", title = "Inventory", description = "Inventory management" },
            new { name = "medicines", title = "Medicines", description = "Medicine list" },
            new { name = "lowstock", title = "Low Stock", description = "Low stock alerts" },
            new { name = "invoices", title = "Invoices", description = "Invoice management" },
            new { name = "statistics", title = "Statistics", description = "Analytics and reports" }
        };

        return Ok(new
        {
            sections = sections,
            totalSections = sections.Length
        });
    }
}

