using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;

namespace WebApplication2.Controllers
{
    public class GymController : Controller
    {
        private readonly AppDbContext context;
        public GymController(AppDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index(int id)
        {
            var gyms = context.Gym.Where(c => c.city_id == id).ToList();
            Console.WriteLine(gyms);
            return Json(gyms);
        }

    }
}
