using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
            var gymAccessories = context.Accessory
            .Select(a => new GymAccessoryView
            {
                AccessoryId = a.id,
                AccessoryName = a.name,
                IsSelected = context.GymAccessory
                    .Any(ga => ga.gym_id == id && ga.accessory_id == a.id)
            })
            .ToList();
            var gymSports = context.Sport
            .Select(a => new GymSportView
            {
                SportId = a.id,
                SportName = a.name,
                IsSelected = context.GymSport
                    .Any(ga => ga.gym_id == id && ga.sport_id == a.id)
            })
            .ToList();
            var model = new EditGym
            {
                gym = gym,
                Accessories = gymAccessories,
                Sports = gymSports
            };
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

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditGym(EditGym model)
        {

            if (ModelState.IsValid)
            {
                var gym = await context.Gym.FindAsync(model.gym.id);
                var gymAccessories = context.GymAccessory.Where(ga => ga.gym_id == model.gym.id);
                var gymAccessoryIds = gymAccessories.Select(ga => ga.accessory_id).ToList();

                var gymSports = context.GymSport.Where(ga => ga.gym_id == model.gym.id);
                var gymSportIds = gymSports.Select(ga => ga.sport_id).ToList();
                if (gym == null)
                {
                    return NotFound();
                }
                // Add new accessories
                foreach (var accessoryId in model.Accessories)
                {
                    if (!gymAccessories.Any(a =>a.accessory_id==accessoryId.AccessoryId) && accessoryId.IsSelected==true)
                    {
                        context.GymAccessory.Add(new GymAccessory { gym_id = model.gym.id, accessory_id = accessoryId.AccessoryId });
                    }
                }
                // Add new sports
                foreach (var sportId in model.Sports)
                {
                    if (!gymSports.Any(a => a.sport_id == sportId.SportId) && sportId.IsSelected == true)
                    {
                        context.GymSport.Add(new GymSport { gym_id = model.gym.id, sport_id = sportId.SportId });
                    }
                }

                //Remove deleted accessories
                foreach (var gymAccessory in gymAccessories)
                {
                    if (model.Accessories.Any(gid => gid.AccessoryId == gymAccessory.accessory_id && gid.IsSelected == false))
                    {
                        context.GymAccessory.Remove(gymAccessory);
                    }
                }
                //Remove deleted sports
                foreach (var gymsport in gymSports)
                {
                    if (model.Sports.Any(gid => gid.SportId == gymsport.sport_id && gid.IsSelected == false))
                    {
                        context.GymSport.Remove(gymsport);
                    }
                }
                gym.name = model.gym.name;
                gym.adress = model.gym.adress;
                gym.description = model.gym.description;
                gym.price = model.gym.price;
                gym.startwork = model.gym.startwork;
                gym.endwork = model.gym.endwork;

                await context.SaveChangesAsync();

                return RedirectToAction("OwnedGyms");
            }

            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> AddGym()
        {
            ViewBag.Cities = await context.City.Select(c => new SelectListItem { Value = c.id.ToString(), Text = c.name }).ToListAsync();
            var model = new AddGym
            {
                Accessories = context.Accessory.ToList(),
                Sports = context.Sport.ToList()
            };
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> AddGym(AddGym gym, List<int> idlis, List<int> idsport)
        {
            if (ModelState.IsValid)
            {

                var user = await _userManager.GetUserAsync(User);
                var newgym = new Gym
                {
                    city_id = gym.city_id,
                    owner_id = user.Id,
                    name = gym.name,
                    description = gym.description,
                    adress = gym.adress,
                    price = gym.price,
                    startwork = gym.startwork,
                    endwork = gym.endwork
                };
                if (user != null)
                {
                   await context.Gym.AddAsync(newgym);
                    context.SaveChanges();
                    foreach (int id in idlis) {
                        var GymAccessor = new GymAccessory
                        {
                            gym_id = newgym.id,
                            accessory_id = id
                        };
                        await context.GymAccessory.AddAsync(GymAccessor);
                    }
                    foreach (int id in idsport)
                    {
                        var GymSport = new GymSport
                        {
                            gym_id = newgym.id,
                            sport_id = id
                        };
                        await context.GymSport.AddAsync(GymSport);
                    }
                    context.SaveChanges();
                    TempData["GoodMessage"] = $"Added new gym with id - {newgym.id}!";
                    return RedirectToAction("OwnedGyms");
                }

            }
            TempData["ErrorMessage"] = "Incorrect data!";
            return RedirectToAction("AddGym");
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
        [HttpGet]
        public  IActionResult GymDetails(int id)
        {
            var gym = context.Gym.Find(id);


            var gymAccessories = context.GymAccessory
                .Where(ga => ga.gym_id == id)
                .Join(context.Accessory,
                      ga => ga.accessory_id,
                      a => a.id,
                      (ga, a) => a);
            var gymSports = context.GymSport
            .Where(ga => ga.gym_id == id)
            .Join(context.Sport,
            ga => ga.sport_id,
            a => a.id,
            (ga, a) => a);

            List<Accessory> accessoryNames = gymAccessories.ToList();
            List<Sport> sportNames = gymSports.ToList();
            var viewModel = new GymView
            {
                Id = id,
                Name = gym.name,
                Address = gym.adress,
                Description = gym.description,
                Price = gym.price,
                Accessories = accessoryNames,
                Sports = sportNames,
                Startwork = gym.startwork,
                Endwork = gym.endwork
            };
            return View(viewModel);
        }
        [HttpGet]
        public IActionResult BookGym(int gymId, DateTime selectedDate)
        {
            var gym = context.Gym.Find(gymId);
            var availableHours = new List<int>();
            DateTime date1 = new();
            if (selectedDate ==  date1)
            {
                selectedDate = DateTime.Now.Date;
            }

            //var bookedHours = context.BookingOrders
            //    .Where(o => o.GymId == gymId && o.BookingDate.Date == selectedDate)
            //    .Select(o => o.BookingHour)
            //    .ToList();


            //for (int hour = gym.startwork; hour <= gym.endwork; hour++)
            //{
            //    if (!bookedHours.Contains(hour))
            //    {
            //        availableHours.Add(hour);
            //    }
            //}
            int startHour = gym.startwork;
            int endHour = gym.endwork;
            var availableSlots = Enumerable.Range(gym.startwork, gym.endwork-gym.startwork)
                .Select(hour => new {
                    BookingDate = selectedDate,
                    BookingHour = hour,
                    AvailableSports = context.GymSport
            .Join(context.Sport, gs => gs.sport_id, s => s.id, (gs, s) => new { Sport = s, GymSport = gs })
            .Where(joined => joined.GymSport.gym_id == gymId && !context.BookingOrders.Any(b => b.GymId == gymId && b.BookedSportid == joined.GymSport.sport_id && b.BookingDate == selectedDate && b.BookingHour == hour))
            .Select(joined => new { SportId = joined.Sport.id, SportName = joined.Sport.name })
            .ToList()
                })
                .ToList();


            ViewBag.AvailableSlots = availableSlots;


            var viewModel = new BookGymViewModel
            {
                GymId = gym.id,
                GymName = gym.name,
                AvailableHours = availableHours,
                SelectedDate = selectedDate
            };

            return View(viewModel);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BookGym(int GymId, DateTime bookingDate, string selectedSlot)
        {
            var selectedValues = selectedSlot.Split('_');
            var selectedHour = int.Parse(selectedValues[0]);
            var selectedSportId = int.Parse(selectedValues[1]);

            var userId = _userManager.GetUserId(User);


          
                var bookingOrder = new BookingOrder
                {
                    GymId = GymId,
                    UserId = userId,
                    BookingDate = bookingDate,
                    BookingHour = selectedHour,
                    OrderDate = DateTime.Now,
                    BookedSportid = selectedSportId

                };


               await context.BookingOrders.AddAsync(bookingOrder);
            
                await context.SaveChangesAsync();

                return RedirectToAction("BookGym", new { gymId = GymId });
            


        }
        [HttpGet]
        [Authorize]
        public IActionResult OrderedGyms()
        {
            var userId = _userManager.GetUserId(User);

            var orders = context.BookingOrders
                    .Where(g => g.UserId == userId)
                    .OrderByDescending(g => g.BookingDate)
                    .ToList();


            return View(orders);
        }


    }
}
