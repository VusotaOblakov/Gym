using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index() {
            var regions = await context.Region.ToListAsync();
            return View(regions);
        }

    }
}
