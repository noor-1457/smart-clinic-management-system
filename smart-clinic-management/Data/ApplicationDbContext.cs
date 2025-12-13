using Microsoft.EntityFrameworkCore;
using smart_clinic_management.Models;

namespace smart_clinic_management.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Patient entity
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.MedicalHistory).HasMaxLength(2000);
                entity.Property(e => e.CreatedDate).IsRequired();
            });

            // Configure Doctor entity
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Specialization).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LicenseNumber).HasMaxLength(50);
                entity.Property(e => e.Bio).HasMaxLength(1000);
                entity.Property(e => e.CreatedDate).IsRequired();
            });

            // Configure Appointment entity
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReasonForVisit).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.CreatedDate).IsRequired();

                // Configure relationships
                entity.HasOne(e => e.Patient)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Doctor)
                    .WithMany(d => d.Appointments)
                    .HasForeignKey(e => e.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Create index on AppointmentDateTime for better query performance
                entity.HasIndex(e => e.AppointmentDateTime);
                entity.HasIndex(e => e.Status);
            });
        }
    }
}

