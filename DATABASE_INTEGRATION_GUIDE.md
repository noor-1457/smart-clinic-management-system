# Database Integration Guide - Smart Clinic Management System

## 📊 Overview

This project uses **Entity Framework Core 8.0** as the ORM (Object-Relational Mapping) framework to integrate with **SQL Server** database. The integration follows a **layered architecture** with two different approaches for MVC and API.

---

## 🏗️ Database Architecture

### Dual Database Context Strategy

The project uses **TWO separate DbContexts** that connect to the **SAME database** but manage different entity sets:

#### 1. **ApplicationDbContext** (MVC - Direct Access)
- **Purpose**: Used by MVC controllers for web pages
- **ID Type**: `int` (auto-incrementing integers)
- **Models Location**: `Models/` folder
- **Access Pattern**: Direct DbContext injection into controllers
- **Entities**: Patient, Doctor, Appointment (simplified models)

#### 2. **ClinicDbContext** (API - Repository Pattern)
- **Purpose**: Used by API controllers through services
- **ID Type**: `Guid` (globally unique identifiers)
- **Entities Location**: `Entities/` folder
- **Access Pattern**: Repository → Service → Controller
- **Entities**: Patient, Doctor, Appointment, Consultation, Prescription, Medicine, Invoice, etc.

---

## 🔌 Database Connection Setup

### 1. Connection String Configuration

**Location**: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartClinicDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Connection String Breakdown**:
- `Server=(localdb)\\mssqllocaldb` - SQL Server LocalDB instance
- `Database=SmartClinicDB` - Database name
- `Trusted_Connection=True` - Windows Authentication
- `MultipleActiveResultSets=true` - Allows multiple queries on same connection
- `TrustServerCertificate=True` - Trusts SSL certificate (for development)

### 2. DbContext Registration

**Location**: `Program.cs`

```csharp
// Register ClinicDbContext for API (Entities with Guid IDs)
builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register ApplicationDbContext for MVC (Models with int IDs)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Key Points**:
- Both contexts use the **same connection string** but can create separate tables
- Registered as **Scoped** services (one instance per HTTP request)
- Entity Framework Core handles connection pooling automatically

---

## 📐 Database Schema Configuration

### ApplicationDbContext Configuration

**File**: `Data/ApplicationDbContext.cs`

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity properties, relationships, and indexes
        // Using Fluent API for configuration
    }
}
```

**Configuration Details**:
- **Primary Keys**: Auto-configured as `int` with identity
- **String Lengths**: Configured via `HasMaxLength()`
- **Required Fields**: Configured via `IsRequired()`
- **Relationships**: Configured with `HasOne().WithMany()`
- **Delete Behavior**: `Restrict` (prevents cascade deletes)
- **Indexes**: Created on `AppointmentDateTime` and `Status` for performance

**Example Relationship Configuration**:
```csharp
entity.HasOne(e => e.Patient)
    .WithMany(p => p.Appointments)
    .HasForeignKey(e => e.PatientId)
    .OnDelete(DeleteBehavior.Restrict);
```

### ClinicDbContext Configuration

**File**: `Data/ClinicDbContext.cs`

```csharp
public class ClinicDbContext : DbContext
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships and decimal precision
    }
}
```

**Configuration Details**:
- **Primary Keys**: Auto-configured as `Guid` (default value: `Guid.NewGuid()`)
- **Decimal Precision**: Configured for money fields (18,2)
- **Cascade Deletes**: Used for child entities (PrescriptionItems, InvoiceItems)
- **Restrict Deletes**: Used for parent entities (Appointment, Patient, Doctor)

**Example Decimal Precision**:
```csharp
modelBuilder.Entity<Medicine>()
    .Property(m => m.PricePerUnit)
    .HasPrecision(18, 2);
```

---

## 🔄 Integration Patterns

### Pattern 1: Direct DbContext Access (MVC)

**Used By**: MVC Controllers (e.g., `AppointmentController`)

**Flow**:
```
Controller → ApplicationDbContext → Database
```

**Example**:
```csharp
public class AppointmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public AppointmentController(ApplicationDbContext context)
    {
        _context = context; // Direct injection
    }

    public async Task<IActionResult> Index()
    {
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .OrderByDescending(a => a.AppointmentDateTime)
            .ToListAsync();
        
        return View(appointments);
    }
}
```

**Characteristics**:
- ✅ Simple and straightforward
- ✅ Direct LINQ queries
- ✅ Eager loading with `.Include()`
- ⚠️ Business logic in controllers (not ideal for complex scenarios)

### Pattern 2: Repository Pattern (API)

**Used By**: API Controllers through Services

**Flow**:
```
API Controller → Service → Repository → ClinicDbContext → Database
```

#### Step 1: Repository Layer

**File**: `Repositories/Repository.cs`

```csharp
public class Repository<T> : IRepository<T> where T : class
{
    private readonly ClinicDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ClinicDbContext context)
    {
        _context = context; // Injected DbContext
        _dbSet = context.Set<T>(); // Get DbSet for entity type T
    }

    // Generic CRUD operations
    public Task<T?> GetByIdAsync(Guid id) => _dbSet.FindAsync(id).AsTask();
    
    public IQueryable<T> Query() => _dbSet.AsQueryable();
    
    public Task AddAsync(T entity) => _dbSet.AddAsync(entity).AsTask();
    
    public void Update(T entity) => _dbSet.Update(entity);
    
    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
```

**Key Features**:
- **Generic**: Works with any entity type
- **Abstraction**: Hides EF Core details from services
- **Testable**: Easy to mock for unit testing
- **Reusable**: One repository for all entities

#### Step 2: Service Layer

**File**: `Services/AppointmentService.cs`

```csharp
public class AppointmentService : IAppointmentService
{
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly IRepository<Doctor> _doctorRepository;
    private readonly IRepository<Patient> _patientRepository;

    public AppointmentService(
        IRepository<Appointment> appointmentRepository,
        IRepository<Doctor> doctorRepository,
        IRepository<Patient> patientRepository)
    {
        // Repositories injected via DI
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
    }

    public async Task<AppointmentResponseDto> CreateAsync(AppointmentCreateDto dto)
    {
        // 1. Validate related entities exist
        var doctor = await _doctorRepository.GetByIdAsync(dto.DoctorId) 
            ?? throw new InvalidOperationException("Doctor not found.");
        
        // 2. Business logic validation
        if (dto.ScheduledAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Appointment time must be in the future.");
        }

        // 3. Check for conflicts using LINQ
        var hasConflict = await _appointmentRepository.Query()
            .AnyAsync(a => a.DoctorId == dto.DoctorId &&
                          a.ScheduledAt == dto.ScheduledAt &&
                          a.Status != AppointmentStatus.Rejected);

        // 4. Create entity
        var appointment = new Appointment { /* ... */ };

        // 5. Save to database
        await _appointmentRepository.AddAsync(appointment);
        await _appointmentRepository.SaveChangesAsync();

        // 6. Map to DTO and return
        return Map(appointment);
    }
}
```

**Key Features**:
- **Business Logic**: Contains validation and business rules
- **Multiple Repositories**: Can use multiple repositories in one service
- **DTO Mapping**: Converts entities to DTOs
- **Exception Handling**: Throws meaningful exceptions

#### Step 3: API Controller

**File**: `Controllers/AppointmentsController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService; // Service injected
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _appointmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByPatient), 
                new { patientId = result.PatientId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
```

**Key Features**:
- **Thin Controllers**: Only handles HTTP concerns
- **Service Dependency**: Uses service, not repository directly
- **Error Handling**: Converts exceptions to HTTP responses

---

## 🔗 Dependency Injection Setup

**Location**: `Program.cs`

```csharp
// 1. Register DbContexts
builder.Services.AddDbContext<ClinicDbContext>(...);
builder.Services.AddDbContext<ApplicationDbContext>(...);

// 2. Register Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// 3. Register Services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();
// ... other services
```

**Lifetime Explanation**:
- **Scoped**: One instance per HTTP request
- **DbContext**: Automatically disposed after request
- **Repository**: Created fresh for each request
- **Service**: Created fresh for each request

**Dependency Chain**:
```
HTTP Request
  ↓
Controller (created)
  ↓
Service (created, needs Repository)
  ↓
Repository (created, needs DbContext)
  ↓
DbContext (created, opens DB connection)
  ↓
Database
```

---

## 📦 Entity Framework Core Features Used

### 1. **Code-First Approach**
- Entities defined as C# classes
- Database schema generated from entities
- Migrations track schema changes

### 2. **LINQ to Entities**
- Write queries in C# instead of SQL
- EF Core translates LINQ to SQL
- Type-safe queries

**Example**:
```csharp
var appointments = await _appointmentRepository.Query()
    .Where(a => a.DoctorId == doctorId)
    .OrderByDescending(a => a.ScheduledAt)
    .ToListAsync();
```

### 3. **Eager Loading**
- Load related entities in one query
- Prevents N+1 query problem

**Example**:
```csharp
var appointments = await _context.Appointments
    .Include(a => a.Patient)  // Load Patient
    .Include(a => a.Doctor)   // Load Doctor
    .ToListAsync();
```

### 4. **Change Tracking**
- EF Core tracks entity changes automatically
- `SaveChangesAsync()` persists all changes

**Example**:
```csharp
var appointment = await _repository.GetByIdAsync(id);
appointment.Status = AppointmentStatus.Approved; // Tracked
_repository.Update(appointment); // Mark as modified
await _repository.SaveChangesAsync(); // Save to DB
```

### 5. **Fluent API Configuration**
- Configure entities without attributes
- More flexible than data annotations

**Example**:
```csharp
modelBuilder.Entity<Appointment>()
    .HasOne(a => a.Doctor)
    .WithMany(d => d.Appointments)
    .HasForeignKey(a => a.DoctorId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

## 🗄️ Database Relationships

### One-to-Many Relationships

**Appointment → Patient**:
```csharp
// One Patient can have many Appointments
Appointment.PatientId → Patient.Id
```

**Appointment → Doctor**:
```csharp
// One Doctor can have many Appointments
Appointment.DoctorId → Doctor.Id
```

**Prescription → PrescriptionItems**:
```csharp
// One Prescription can have many Items
PrescriptionItem.PrescriptionId → Prescription.Id
// Cascade delete: If Prescription deleted, Items deleted too
```

**Invoice → InvoiceItems**:
```csharp
// One Invoice can have many Items
InvoiceItem.InvoiceId → Invoice.Id
// Cascade delete: If Invoice deleted, Items deleted too
```

### Navigation Properties

Entities have navigation properties for easy access:

```csharp
public class Appointment
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; } // Navigation property
    
    public Guid DoctorId { get; set; }
    public Doctor? Doctor { get; set; } // Navigation property
}
```

**Usage**:
```csharp
var appointment = await _context.Appointments
    .Include(a => a.Patient)
    .FirstOrDefaultAsync();

string patientName = appointment.Patient.FullName; // Access related entity
```

---

## 🔄 Transaction Management

### Automatic Transactions

Entity Framework Core automatically wraps `SaveChangesAsync()` in a transaction:

```csharp
// All changes saved atomically
await _appointmentRepository.AddAsync(appointment);
await _prescriptionRepository.AddAsync(prescription);
await _invoiceRepository.AddAsync(invoice);
await _appointmentRepository.SaveChangesAsync(); // Single transaction
```

### Manual Transactions

For complex operations:

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Multiple operations
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

## 📊 Database Migrations

### Creating Migrations

```bash
# For ApplicationDbContext
dotnet ef migrations add InitialCreate --context ApplicationDbContext

# For ClinicDbContext
dotnet ef migrations add InitialCreate --context ClinicDbContext
```

### Applying Migrations

```bash
# Create/Update database
dotnet ef database update --context ApplicationDbContext
dotnet ef database update --context ClinicDbContext
```

### Migration Files

Migrations are stored in `Migrations/` folder and contain:
- `Up()` method: Applies changes
- `Down()` method: Reverts changes
- Snapshot of current model state

---

## 🔍 Query Examples

### Simple Query
```csharp
var patient = await _context.Patients
    .FirstOrDefaultAsync(p => p.Id == patientId);
```

### Query with Filtering
```csharp
var appointments = await _appointmentRepository.Query()
    .Where(a => a.Status == AppointmentStatus.Pending)
    .ToListAsync();
```

### Query with Joins (Eager Loading)
```csharp
var appointments = await _context.Appointments
    .Include(a => a.Patient)
    .Include(a => a.Doctor)
    .Where(a => a.DoctorId == doctorId)
    .ToListAsync();
```

### Query with Projection (DTO Mapping)
```csharp
var appointments = await _appointmentRepository.Query()
    .Select(a => new AppointmentResponseDto
    {
        Id = a.Id,
        PatientName = a.Patient.FullName,
        DoctorName = a.Doctor.FullName,
        ScheduledAt = a.ScheduledAt
    })
    .ToListAsync();
```

---

## ⚡ Performance Considerations

### 1. **Async/Await**
All database operations use async methods:
```csharp
await _context.Patients.ToListAsync();
await _repository.SaveChangesAsync();
```

### 2. **Eager Loading**
Prevents N+1 queries:
```csharp
// Good: One query
.Include(a => a.Patient).Include(a => a.Doctor)

// Bad: N+1 queries
// Loading Patient and Doctor separately
```

### 3. **Indexes**
Configured on frequently queried columns:
```csharp
entity.HasIndex(e => e.AppointmentDateTime);
entity.HasIndex(e => e.Status);
```

### 4. **Query Filtering**
Filter at database level, not in memory:
```csharp
// Good: Filtered in SQL
.Where(a => a.Status == AppointmentStatus.Pending)

// Bad: Loads all, filters in memory
.ToList().Where(a => a.Status == AppointmentStatus.Pending)
```

---

## 🛡️ Error Handling

### Database Exceptions

```csharp
try
{
    await _repository.SaveChangesAsync();
}
catch (DbUpdateException ex)
{
    // Handle database update errors
    _logger.LogError(ex, "Database update failed");
    throw;
}
catch (DbUpdateConcurrencyException ex)
{
    // Handle concurrency conflicts
    _logger.LogWarning(ex, "Concurrency conflict detected");
    throw;
}
```

### Validation Errors

```csharp
if (!ModelState.IsValid)
{
    return BadRequest(ModelState);
}
```

---

## 📝 Summary

### Integration Flow Diagram

```
┌─────────────────────────────────────────────────────────┐
│                    HTTP Request                         │
└────────────────────┬────────────────────────────────────┘
                     │
         ┌───────────┴───────────┐
         │                       │
    ┌────▼────┐            ┌─────▼─────┐
    │   MVC   │            │    API    │
    │Controller│            │ Controller│
    └────┬────┘            └─────┬─────┘
         │                       │
         │                       │
    ┌────▼──────────┐      ┌────▼──────┐
    │Application    │      │  Service  │
    │DbContext      │      │   Layer   │
    │(Direct Access)│      └─────┬──────┘
    └────┬──────────┘            │
         │                  ┌────▼──────┐
         │                  │ Repository│
         │                  │   Layer   │
         │                  └─────┬──────┘
         │                       │
         └───────────┬───────────┘
                     │
              ┌──────▼──────┐
              │ClinicDbContext│
              └──────┬──────┘
                     │
              ┌──────▼──────┐
              │   SQL Server │
              │   Database   │
              └─────────────┘
```

### Key Takeaways

1. **Two DbContexts**: One for MVC (int IDs), one for API (Guid IDs)
2. **Repository Pattern**: Used for API to abstract data access
3. **Service Layer**: Contains business logic and validation
4. **Dependency Injection**: All components registered in Program.cs
5. **Entity Framework Core**: Handles all database operations
6. **Code-First**: Database schema generated from entities
7. **Async Operations**: All database calls are asynchronous
8. **Transaction Management**: Automatic transactions on SaveChanges

---

## 🚀 Next Steps

1. **Create Migrations**: Generate database schema
2. **Seed Data**: Add initial data (patients, doctors)
3. **Add Indexes**: Optimize frequently queried columns
4. **Add Logging**: Log database operations
5. **Add Caching**: Cache frequently accessed data
6. **Add Unit Tests**: Test repository and service layers

