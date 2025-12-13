using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using smart_clinic_management.Data;
using smart_clinic_management.Models;

namespace smart_clinic_management.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(ApplicationDbContext context, ILogger<AppointmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Appointment/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Load patients and doctors for dropdown lists
                var patients = await _context.Patients
                    .OrderBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .ToListAsync();

                var doctors = await _context.Doctors
                    .OrderBy(d => d.LastName)
                    .ThenBy(d => d.FirstName)
                    .ToListAsync();

                ViewBag.PatientId = new SelectList(patients, "Id", "FullName");
                ViewBag.DoctorId = new SelectList(doctors, "Id", "FullName");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create appointment form");
                TempData["ErrorMessage"] = "An error occurred while loading the form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientId,DoctorId,AppointmentDateTime,ReasonForVisit,Notes")] Appointment appointment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validate appointment date is in the future
                    if (appointment.AppointmentDateTime <= DateTime.Now)
                    {
                        ModelState.AddModelError("AppointmentDateTime", "Appointment date and time must be in the future.");
                    }

                    // Validate patient exists
                    var patientExists = await _context.Patients.AnyAsync(p => p.Id == appointment.PatientId);
                    if (!patientExists)
                    {
                        ModelState.AddModelError("PatientId", "Selected patient does not exist.");
                    }

                    // Validate doctor exists
                    var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == appointment.DoctorId);
                    if (!doctorExists)
                    {
                        ModelState.AddModelError("DoctorId", "Selected doctor does not exist.");
                    }

                    // Check for conflicting appointments (same doctor at same time)
                    var conflictingAppointment = await _context.Appointments
                        .Where(a => a.DoctorId == appointment.DoctorId
                                 && a.AppointmentDateTime == appointment.AppointmentDateTime
                                 && a.Status != "Rejected" && a.Status != "Completed")
                        .FirstOrDefaultAsync();

                    if (conflictingAppointment != null)
                    {
                        ModelState.AddModelError("AppointmentDateTime", "The doctor already has an appointment at this time.");
                    }

                    if (ModelState.IsValid)
                    {
                        appointment.Status = "Pending";
                        appointment.CreatedDate = DateTime.Now;
                        appointment.UpdatedDate = null;

                        _context.Add(appointment);
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Appointment created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Reload dropdowns if validation fails
                var patients = await _context.Patients
                    .OrderBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .ToListAsync();

                var doctors = await _context.Doctors
                    .OrderBy(d => d.LastName)
                    .ThenBy(d => d.FirstName)
                    .ToListAsync();

                ViewBag.PatientId = new SelectList(patients, "Id", "FullName", appointment.PatientId);
                ViewBag.DoctorId = new SelectList(doctors, "Id", "FullName", appointment.DoctorId);

                return View(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                TempData["ErrorMessage"] = "An error occurred while creating the appointment. Please try again.";
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: Appointment/UpdateStatus/5?status=Approved
        public async Task<IActionResult> UpdateStatus(int? id, string? status)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (appointment == null)
                {
                    return NotFound();
                }

                // If status is provided, update it directly
                if (!string.IsNullOrEmpty(status) && IsValidStatus(status))
                {
                    appointment.Status = status;
                    appointment.UpdatedDate = DateTime.Now;

                    _context.Update(appointment);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Appointment status updated to {status} successfully!";
                    
                    // Redirect based on referer or default to Index
                    var referer = Request.Headers["Referer"].ToString();
                    if (referer.Contains("DoctorAppointments"))
                    {
                        return RedirectToAction(nameof(DoctorAppointments), new { doctorId = appointment.DoctorId });
                    }
                    else if (referer.Contains("PatientHistory"))
                    {
                        return RedirectToAction(nameof(PatientHistory), new { patientId = appointment.PatientId });
                    }
                    
                    return RedirectToAction(nameof(Index));
                }

                // Otherwise, show the update form
                ViewBag.StatusList = new SelectList(new[]
                {
                    new { Value = "Pending", Text = "Pending" },
                    new { Value = "Approved", Text = "Approved" },
                    new { Value = "Rejected", Text = "Rejected" },
                    new { Value = "Completed", Text = "Completed" }
                }, "Value", "Text", appointment.Status);

                return View(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading update status form for appointment {AppointmentId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the update form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Appointment/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, [Bind("Id,Status")] Appointment appointmentUpdate)
        {
            if (id != appointmentUpdate.Id)
            {
                return NotFound();
            }

            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                {
                    return NotFound();
                }

                if (!IsValidStatus(appointmentUpdate.Status))
                {
                    ModelState.AddModelError("Status", "Invalid status value.");
                    ViewBag.StatusList = new SelectList(new[]
                    {
                        new { Value = "Pending", Text = "Pending" },
                        new { Value = "Approved", Text = "Approved" },
                        new { Value = "Rejected", Text = "Rejected" },
                        new { Value = "Completed", Text = "Completed" }
                    }, "Value", "Text", appointmentUpdate.Status);
                    return View(appointment);
                }

                appointment.Status = appointmentUpdate.Status;
                appointment.UpdatedDate = DateTime.Now;

                _context.Update(appointment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Appointment status updated to {appointmentUpdate.Status} successfully!";

                // Smart redirect based on referer
                var referer = Request.Headers["Referer"].ToString();
                if (referer.Contains("DoctorAppointments"))
                {
                    return RedirectToAction(nameof(DoctorAppointments), new { doctorId = appointment.DoctorId });
                }
                else if (referer.Contains("PatientHistory"))
                {
                    return RedirectToAction(nameof(PatientHistory), new { patientId = appointment.PatientId });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(appointmentUpdate.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment status for appointment {AppointmentId}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the appointment status. Please try again.";
                return RedirectToAction(nameof(UpdateStatus), new { id });
            }
        }

        // GET: Appointment/DoctorAppointments
        public async Task<IActionResult> DoctorAppointments(int? doctorId)
        {
            try
            {
                // If no doctor selected, show selection page
                if (doctorId == null)
                {
                    var doctors = await _context.Doctors
                        .OrderBy(d => d.LastName)
                        .ThenBy(d => d.FirstName)
                        .ToListAsync();

                    ViewBag.DoctorId = new SelectList(doctors, "Id", "FullName");
                    return View("SelectDoctor");
                }

                // Validate doctor exists
                var doctor = await _context.Doctors.FindAsync(doctorId);
                if (doctor == null)
                {
                    TempData["ErrorMessage"] = "Doctor not found.";
                    return RedirectToAction(nameof(DoctorAppointments));
                }

                // Get all appointments for the selected doctor
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Where(a => a.DoctorId == doctorId)
                    .OrderByDescending(a => a.AppointmentDateTime)
                    .ToListAsync();

                ViewBag.DoctorName = doctor.FullName;
                ViewBag.DoctorId = doctorId;

                return View(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctor appointments for doctor {DoctorId}", doctorId);
                TempData["ErrorMessage"] = "An error occurred while fetching appointments. Please try again.";
                return RedirectToAction(nameof(DoctorAppointments));
            }
        }

        // POST: Appointment/DoctorAppointments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoctorAppointments(int doctorId)
        {
            return RedirectToAction(nameof(DoctorAppointments), new { doctorId });
        }

        // GET: Appointment/PatientHistory
        public async Task<IActionResult> PatientHistory(int? patientId)
        {
            try
            {
                // If no patient selected, show selection page
                if (patientId == null)
                {
                    var patients = await _context.Patients
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName)
                        .ToListAsync();

                    ViewBag.PatientId = new SelectList(patients, "Id", "FullName");
                    return View("SelectPatient");
                }

                // Validate patient exists
                var patient = await _context.Patients.FindAsync(patientId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found.";
                    return RedirectToAction(nameof(PatientHistory));
                }

                // Get all appointments for the selected patient
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Where(a => a.PatientId == patientId)
                    .OrderByDescending(a => a.AppointmentDateTime)
                    .ToListAsync();

                ViewBag.PatientName = patient.FullName;
                ViewBag.PatientId = patientId;

                return View(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching patient history for patient {PatientId}", patientId);
                TempData["ErrorMessage"] = "An error occurred while fetching patient history. Please try again.";
                return RedirectToAction(nameof(PatientHistory));
            }
        }

        // POST: Appointment/PatientHistory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PatientHistory(int patientId)
        {
            return RedirectToAction(nameof(PatientHistory), new { patientId });
        }

        // GET: Appointment/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .OrderByDescending(a => a.AppointmentDateTime)
                    .ToListAsync();

                return View(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointments");
                TempData["ErrorMessage"] = "An error occurred while fetching appointments. Please try again.";
                return View(new List<Appointment>());
            }
        }

        // GET: Appointment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (appointment == null)
                {
                    return NotFound();
                }

                return View(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointment details for appointment {AppointmentId}", id);
                TempData["ErrorMessage"] = "An error occurred while fetching appointment details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper methods
        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }

        private bool IsValidStatus(string status)
        {
            return status == "Pending" || status == "Approved" || status == "Rejected" || status == "Completed";
        }
    }
}

