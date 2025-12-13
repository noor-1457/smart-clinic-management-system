using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Specialization is required")]
        [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters")]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "License number cannot exceed 50 characters")]
        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }

        [Display(Name = "Years of Experience")]
        [Range(0, 100, ErrorMessage = "Years of experience must be between 0 and 100")]
        public int? YearsOfExperience { get; set; }

        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters")]
        public string? Bio { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";
    }
}

