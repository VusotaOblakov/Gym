using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebApplication2.Data;

namespace WebApplication2.Controllers
{
    public class CityController : Controller
    {
        private readonly AppDbContext context;
        public CityController(AppDbContext context)
        {
            this.context = context;
        }

        //Отримання міст по області в JSON
        public async Task<IActionResult> GetCitiesByRegion(int region_id)
        {
            var cities = await context.City.Where(c => c.region_id == region_id).ToListAsync();
            return Json(cities);
        }

    }
}
