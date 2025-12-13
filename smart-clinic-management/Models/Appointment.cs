using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace smart_clinic_management.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Patient is required")]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Doctor is required")]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Appointment date and time is required")]
        [Display(Name = "Appointment Date & Time")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDateTime { get; set; }

        [Required(ErrorMessage = "Reason for visit is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        [Display(Name = "Reason for Visit")]
        public string ReasonForVisit { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Completed

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Updated Date")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;

        // Helper property for UI styling
        [NotMapped]
        public string StatusBadgeClass
        {
            get
            {
                return Status switch
                {
                    "Approved" => "bg-success",
                    "Rejected" => "bg-danger",
                    "Completed" => "bg-info",
                    "Pending" => "bg-warning",
                    _ => "bg-secondary"
                };
            }
        }
    }
}

