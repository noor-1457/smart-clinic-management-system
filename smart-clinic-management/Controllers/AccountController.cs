using Microsoft.AspNetCore.Mvc;

namespace smart_clinic_management.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult DoctorLogin()
        {
            return View();
        }

        public IActionResult AdminLogin()
        {
            return View();
        }
    }
}
