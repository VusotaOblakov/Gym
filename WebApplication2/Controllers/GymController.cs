using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class GymController : Controller
    {
        private readonly AppDbContext context;
        private readonly UserManager<IdentityUser> _userManager;
        public GymController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            _userManager = userManager;
        }

        public IActionResult Index(int id)
        {
            var gyms = context.Gym.Where(c => c.city_id == id).ToList();
            return Json(gyms);
        }
        [Authorize(Roles ="SuperAdmin")]
        public IActionResult AllGyms()
        {
            var gyms = context.Gym.ToList();
            return View(gyms);
        }
        [HttpGet]
        public async Task<IActionResult> EditGym(int id)
        {
            var gym = await context.Gym.FindAsync(id);

            string user =  _userManager.GetUserId(User);
            if (gym == null)
            {
                return NotFound();
            }
            if (!(User.IsInRole("Admin") && gym.owner_id == user || User.IsInRole("SuperAdmin")))
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this gym.";
                return RedirectToAction("AllGyms");
            }

            return View(gym);
        }
        [HttpPost]
        public async Task<IActionResult> EditGym(EditGym model)
        {
            if (ModelState.IsValid)
            {
                var gym = await context.Gym.FindAsync(model.id);

                if (gym == null)
                {
                    return NotFound();
                }

                gym.name = model.name;
                gym.adress = model.adress;
                gym.description = model.description;

                await context.SaveChangesAsync();

                return RedirectToAction(nameof(AllGyms));
            }

            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public IActionResult AddGym()
        {
            ViewBag.Cities = context.City.Select(c => new SelectListItem { Value = c.id.ToString(), Text = c.name }).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddGym(AddGym gym)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine("mode");
                var user = await _userManager.GetUserAsync(User);
                var newgym = new Gym
                {
                    city_id = gym.city_id,
                    owner_id = user.Id,
                    name = gym.name,
                    description = gym.description,
                    adress = gym.adress
                };
                if (user != null)
                {
                   await context.Gym.AddAsync(newgym);
                    context.SaveChanges();
                    return RedirectToAction("OwnedGyms");
                }

            }

            return View(gym);
        }
        public IActionResult DeleteGym(int id)
        {
            string user = _userManager.GetUserId(User);
            var gym = context.Gym.Find(id);
            if (!(User.IsInRole("Admin") && gym.owner_id == user || User.IsInRole("SuperAdmin")))
            {
                TempData["ErrorMessage"] = "You do not have permission to delete this gym.";
                return RedirectToAction("AllGyms");
            }
            if (gym == null)
            {
                return NotFound();
            }

            context.Gym.Remove(gym);
            context.SaveChanges();

            return RedirectToAction("AllGyms");
        }
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult OwnedGyms()
        {
            var userId = _userManager.GetUserId(User);

            var gyms = context.Gym.Where(g => g.owner_id == userId).ToList();

            return View(gyms);
        }

    }
}
