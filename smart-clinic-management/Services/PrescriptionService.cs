using Microsoft.EntityFrameworkCore;
using smart_clinic_management.Data;
using smart_clinic_management.DTOs;
using smart_clinic_management.Entities;
using smart_clinic_management.Repositories;

namespace smart_clinic_management.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly ClinicDbContext _context;
    private readonly IRepository<Prescription> _prescriptionRepository;
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly IRepository<Doctor> _doctorRepository;
    private readonly IRepository<Patient> _patientRepository;
    private readonly IRepository<Medicine> _medicineRepository;
    private readonly IInventoryService _inventoryService;

    public PrescriptionService(
        ClinicDbContext context,
        IRepository<Prescription> prescriptionRepository,
        IRepository<Appointment> appointmentRepository,
        IRepository<Doctor> doctorRepository,
        IRepository<Patient> patientRepository,
        IRepository<Medicine> medicineRepository,
        IInventoryService inventoryService)
    {
        _context = context;
        _prescriptionRepository = prescriptionRepository;
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
        _medicineRepository = medicineRepository;
        _inventoryService = inventoryService;
    }

    public async Task<PrescriptionResponseDto> CreateAsync(PrescriptionCreateDto dto)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId) ?? throw new InvalidOperationException("Appointment not found.");
        var doctor = await _doctorRepository.GetByIdAsync(dto.DoctorId) ?? throw new InvalidOperationException("Doctor not found.");
        var patient = await _patientRepository.GetByIdAsync(dto.PatientId) ?? throw new InvalidOperationException("Patient not found.");

        if (appointment.Status is AppointmentStatus.Pending or AppointmentStatus.Rejected)
        {
            throw new InvalidOperationException("Prescription can only be created for approved or completed appointments.");
        }

        // Ensure the prescription is associated to the correct participants
        if (appointment.DoctorId != doctor.Id || appointment.PatientId != patient.Id)
        {
            throw new InvalidOperationException("Appointment must belong to the same doctor and patient.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var prescription = new Prescription
        {
            AppointmentId = appointment.Id,
            DoctorId = doctor.Id,
            PatientId = patient.Id
        };

        await _prescriptionRepository.AddAsync(prescription);
        await _prescriptionRepository.SaveChangesAsync();

        var items = new List<PrescriptionItem>();

        foreach (var itemDto in dto.Items)
        {
            var medicine = await _medicineRepository.GetByIdAsync(itemDto.MedicineId) ?? throw new InvalidOperationException("Medicine not found.");

            await _inventoryService.DeductStockAsync(medicine.Id, itemDto.Quantity);

            var prescriptionItem = new PrescriptionItem
            {
                PrescriptionId = prescription.Id,
                MedicineId = medicine.Id,
                Dosage = itemDto.Dosage,
                Quantity = itemDto.Quantity,
                Instructions = itemDto.Instructions
            };

            items.Add(prescriptionItem);
        }

        await _context.PrescriptionItems.AddRangeAsync(items);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        prescription.Items = items;

        return Map(prescription, items);
    }

    private PrescriptionResponseDto Map(Prescription prescription, IEnumerable<PrescriptionItem> items) => new()
    {
        Id = prescription.Id,
        AppointmentId = prescription.AppointmentId,
        DoctorId = prescription.DoctorId,
        PatientId = prescription.PatientId,
        CreatedAt = prescription.CreatedAt,
        Items = items.Select(i => new PrescriptionItemResponseDto
        {
            MedicineId = i.MedicineId,
            MedicineName = i.Medicine?.Name ?? string.Empty,
            Quantity = i.Quantity,
            Dosage = i.Dosage,
            Instructions = i.Instructions
        }).ToList()
    };
}

