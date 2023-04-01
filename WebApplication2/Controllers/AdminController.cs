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

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult EditAccessory()
        {

            var  accessories = context.Accessory.ToList();
            var model = new AccessoryView
            {
                Accessories = accessories,
            };

            return View(model);
        }
        public IActionResult EditSport()
        {

            var sports = context.Sport.ToList();
            var model = new SportView
            {
                Sports = sports,
            };

            // Return the view with the view model
            return View(model);
        }
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
        [HttpPost]
        public IActionResult DeleteSport(int sportId)
        {
            var sportToDelete = context.Sport.Find(sportId);
            if (sportToDelete == null)
            {
                return NotFound();
            }

            context.Sport.Remove(sportToDelete);
            context.SaveChanges();

            return RedirectToAction("EditSport");
        }
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
            context.SaveChanges();
            return RedirectToAction("EditAccessory");
        }
        [HttpPost]
        public IActionResult DeleteAccessory(int accessoryId)
        {
            var accessoryToDelete = context.Accessory.Find(accessoryId);
            if (accessoryToDelete == null)
            {
                return NotFound();
            }

            context.Accessory.Remove(accessoryToDelete);
            context.SaveChanges();

            return RedirectToAction("EditAccessory");
        }

        public IActionResult EditRoles()
        {
            var roles = _roleManager.Roles.ToList();
            var model = new EditRolesViewModel
            {
                Roles = roles
            };

            return View(model);
        }
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

        public IActionResult AllUsers()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserRoleViewModel>();
            foreach (var user in users)
            {
                var roles = _userManager.GetRolesAsync(user).Result;
                var roleName = roles.FirstOrDefault();
                model.Add(new UserRoleViewModel
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Role = roleName
                });
            }
            return View(model);
        }

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
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                LockoutEnd = user.LockoutEnd
            };

            ViewBag.Roles = _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToList();

            return View(model);
        }
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
                return RedirectToAction(nameof(AllUsers));
            }

            return RedirectToAction(nameof(AllUsers));
        }

    }
}
