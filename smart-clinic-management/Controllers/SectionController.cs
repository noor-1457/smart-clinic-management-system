using Microsoft.AspNetCore.Mvc;

namespace smart_clinic_management.Controllers
{
    public class SectionController : Controller
    {
        // GET: Section/Show?section=appointments
        // This will be called via onClick from JavaScript
        public IActionResult Show(string section)
        {
            // Return different partial views based on the section parameter
            return section?.ToLower() switch
            {
                "appointments" => PartialView("_AppointmentsSection"),
                "patients" => PartialView("_PatientsSection"),
                "doctors" => PartialView("_DoctorsSection"),
                "dashboard" => PartialView("_DashboardSection"),
                "statistics" => PartialView("_StatisticsSection"),
                "reports" => PartialView("_ReportsSection"),
                _ => PartialView("_DefaultSection")
            };
        }

        // GET: Section/Modal?type=create&entity=appointment
        // For showing modals on click
        public IActionResult Modal(string type, string entity)
        {
            ViewBag.ModalType = type;
            ViewBag.Entity = entity;
            
            return PartialView("_Modal", new { Type = type, Entity = entity });
        }

        // GET: Section/QuickView?id=123&type=appointment
        // For quick view popups
        public IActionResult QuickView(int id, string type)
        {
            ViewBag.Id = id;
            ViewBag.Type = type;
            
            return PartialView("_QuickView");
        }

        // GET: Section/TogglePanel?panel=sidebar
        // For toggling UI panels
        public IActionResult TogglePanel(string panel)
        {
            ViewBag.PanelName = panel;
            return PartialView("_TogglePanel");
        }
    }
}

