using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext context;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            this.context = context;
        }
        //Меню з пунктами адмінки
        public IActionResult Index()
        {

            return View();
        }
       //Редагування Принадлежностей(СRUD)
        public async Task<IActionResult> EditAccessory()
        {

            var  accessories = await context.Accessory.ToListAsync();
            var model = new AccessoryView
            {
                Accessories = accessories,
            };

            return View(model);
        }
        //Редагування Спорту(СRUD)
        public async Task<IActionResult> EditSport()
        {

            var sports = await context.Sport.ToListAsync();
            var model = new SportView
            {
                Sports = sports,
            };

            return View(model);
        }
        //Додавання нового спорту
        [HttpPost]
        public async Task<IActionResult> AddSport(string SportName)
        {
            var sport = new Sport
            {
                name = SportName,
            };

            await context.Sport.AddAsync(sport);
            context.SaveChanges();
            return RedirectToAction("EditSport");
        }
        //Видалення  спорту
        [HttpPost]
        public async Task<IActionResult> DeleteSport(int sportId)
        {
            var sportToDelete = await context.Sport.FindAsync(sportId);
            if (sportToDelete == null)
            {
                return NotFound();
            }

            context.Sport.Remove(sportToDelete);
            await context.SaveChangesAsync();

            return RedirectToAction("EditSport");
        }
        //Додавання нової принадлежності
        [HttpPost]
        public async Task<IActionResult> AddAccessory(string AccessoryName)
        {
            var accessory = new Accessory
            {
                name = AccessoryName,
            };
            if (string.IsNullOrWhiteSpace(AccessoryName))
            {
                ModelState.AddModelError("", "Accessory name can't be null or whitespace.");
                return RedirectToAction("EditAccessory");
            }
            await context.Accessory.AddAsync(accessory);
            await context.SaveChangesAsync();
            return RedirectToAction("EditAccessory");
        }
        //Видалення принадлежності
        [HttpPost]
        public async Task<IActionResult> DeleteAccessory(int accessoryId)
        {
            var accessoryToDelete = await context.Accessory.FindAsync(accessoryId);
            if (accessoryToDelete == null)
            {
                return NotFound();
            }

            context.Accessory.Remove(accessoryToDelete);
            await context.SaveChangesAsync();

            return RedirectToAction("EditAccessory");
        }
        //Редагування Ролей
        public async Task<IActionResult> EditRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var model = new EditRolesViewModel
            {
                Roles = roles
            };

            return View(model);
        }
        //Додавання Ролей
        [HttpPost]
        public async Task<IActionResult> AddRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ModelState.AddModelError("", "Role name can't be null or whitespace.");
                return View("EditRoles");
            }

            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("EditRoles");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("EditRoles");
            }
        }
        //Видалення Ролі
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                return NotFound($"Role '{roleName}' not found.");
            }

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(EditRoles));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(nameof(EditRoles), new EditRolesViewModel
            {
                Roles = await _roleManager.Roles.ToListAsync()
            });
        }
        //Список всіх користувачів
        public async Task<IActionResult> AllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserRoleViewModel>();
            foreach (var user in users)
            {
                var roles = _userManager.GetRolesAsync(user).Result;
                var roleName = roles.FirstOrDefault();
                model.Add(new UserRoleViewModel
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Role = roleName!
                });
            }
            return View(model);
        }
        //Редагування Користувача
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserRoleViewModel
            {
                Id = user.Id,
                Name = user.UserName,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()!,
                LockoutEnd = user.LockoutEnd
            };

            ViewBag.Roles = await _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToListAsync();

            return View(model);
        }
        //Блокування користувача
        [HttpPost]
        public async Task<IActionResult> BanUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            return RedirectToAction("EditUser", new { id = id });
        }
        //Розблокування користувача
        [HttpPost]
        public async Task<IActionResult> UnBanUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            return RedirectToAction("EditUser", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.Name;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to update user");
                return View(model);
            }

            var currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            Console.WriteLine(currentRole);
            if (currentRole != model.Role)
            {
                if (currentRole != null)
                {
                    await _userManager.RemoveFromRoleAsync(user, currentRole);
                }
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            return RedirectToAction("AllUsers");
        }
        //Видалення користувача
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound($"user '{user}' not found.");
            }
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("AllUsers");
            }

            return RedirectToAction("AllUsers");
        }
        [HttpGet]
        public async Task<IActionResult> Report(DateTime selectedDate, DateTime endDate, int cityId, List<int> idsport, decimal? minPrice, decimal? maxPrice, string gymName)
        {
            ViewBag.Cities = await context.City.Select(c => new SelectListItem { Value = c.id.ToString(), Text = c.name }).ToListAsync();
            ViewBag.Accessory = await context.Accessory.ToListAsync();
            ViewBag.Sport = await context.Sport.ToListAsync();

            if (selectedDate == default)
            {
                selectedDate = DateTime.MinValue;
                endDate = DateTime.MaxValue;
            }

            if (cityId > 0)
            {
                var cityname = (await context.City.FindAsync(cityId))?.name;
                TempData["Message"] = $"Choosen city {cityname} from {selectedDate.ToString("dd/MM/yyyy")} to {endDate.ToString("dd/MM/yyyy")}";
            }
            else
            {
                TempData["Message"] = $"Choosen orders from {selectedDate.ToString("dd/MM/yyyy")} to {endDate.ToString("dd/MM/yyyy")}";
            }

            var bookingsQuery = context.BookingOrders
                .Where(bo => bo.BookingDate >= selectedDate && bo.BookingDate <= endDate)
                .Where(bo => cityId == 0 || context.Gym.Any(g => g.city_id == cityId && g.id == bo.GymId))
                .Where(bo => !idsport.Any() || idsport.Contains(bo.BookedSportid));

            if (minPrice.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(bo => context.Gym.Any(g => g.id == bo.GymId && g.price >= minPrice.Value));
                TempData["Message"] += $". Minimal price - {minPrice}";
            }

            if (maxPrice.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(bo => context.Gym.Any(g => g.id == bo.GymId && g.price <= maxPrice.Value));
                TempData["Message"] += $". Maximal price - {maxPrice}";
            }

            if (!string.IsNullOrWhiteSpace(gymName))
            {
                var gymNameLower = gymName.ToLower();
                bookingsQuery = bookingsQuery.Where(bo => context.Gym.Any(g => g.id == bo.GymId && g.name.ToLower().Contains(gymNameLower)));
                TempData["Message"] += $". With name {gymName}";
            }

            var bookings = await bookingsQuery.ToListAsync();
            return View(bookings);
        }
    }
}
