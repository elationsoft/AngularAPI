using Microsoft.AspNetCore.Mvc;

namespace WebAPIsAngular.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
