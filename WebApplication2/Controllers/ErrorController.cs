using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult NotFound()
        {
            return View();
        }
    }
}
