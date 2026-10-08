using Microsoft.AspNetCore.Mvc;

namespace ChordFinderAPI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
