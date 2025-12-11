using Microsoft.AspNetCore.Mvc;

namespace smart_clinic_management.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
