using System.ComponentModel.DataAnnotations;

namespace smart_clinic_management.Models
{
    public class Patient
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

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
        public string? Gender { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }

        [Display(Name = "Medical History")]
        [StringLength(2000, ErrorMessage = "Medical history cannot exceed 2000 characters")]
        public string? MedicalHistory { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";
    }
}

