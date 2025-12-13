using Microsoft.AspNetCore.Mvc;
using smart_clinic_management.Models;   //  Add this line for using DoctorViewModel
using System.Collections.Generic;

namespace smart_clinic_management.Controllers
{
    public class HomeController : Controller
    {
        // HOME PAGE
        public IActionResult Index()
        {
            return View();
        }

        //  DOCTORS LISTING PAGE 
        public IActionResult Doctors()
        {
            // Temporary static list (Later DB se aayega)
            var doctors = new List<DoctorViewModel>
            {
                new DoctorViewModel { Name = "Dr. Ali Khan", Specialty = "Cardiologist", Experience = "10 years" },
                new DoctorViewModel { Name = "Dr. Sara Ahmed", Specialty = "Dentist", Experience = "5 years" },
                new DoctorViewModel { Name = "Dr. Usman Tariq", Specialty = "Dermatologist", Experience = "7 years" }
            };

            return View(doctors);
        }

        public IActionResult Contact()
        {
            return View();
        }

    }
}
