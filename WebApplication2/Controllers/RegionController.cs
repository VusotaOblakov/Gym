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


        //Вивід всіх областей
        public  IActionResult Index() {
            var regions =  context.Region.ToList();
            return View(regions);
        }

    }
}
