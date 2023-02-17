using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        // GET: RegionController'


        public IActionResult GetCitiesByRegion(int region_id)
        {
            var cities = context.City.Where(c => c.region_id == region_id).ToList();
            return Json(cities);
        }

    }
}
