using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;

namespace WebApplication2.Controllers
{
    public class RegionController : Controller
    {
        private readonly AppDbContext context;
        public RegionController(AppDbContext context)
        {
            this.context = context;
        }
        // GET: RegionController'


        public  IActionResult Index() {
            var regions =  context.Region.ToList();
            return View(regions);
        }
        public IActionResult TestPage()
        {
            return View();
        }
    }
}
