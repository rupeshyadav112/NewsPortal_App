using Microsoft.AspNetCore.Mvc;

namespace NewsPortal_App.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View("About");
        }
    }
}
