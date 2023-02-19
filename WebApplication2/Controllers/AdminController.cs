using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {

            return View();
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
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
            };

            ViewBag.Roles = _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToList();

            return View(model);
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
