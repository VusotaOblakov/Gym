using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly GoogleCalendarService _gCalendar;
        public GymController(AppDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment, GoogleCalendarService gCalendar)
        {
            this.context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _gCalendar = gCalendar;
        }
        //Повертає список залів в місті в JSON
        public async Task<IActionResult> Index(int id)
        {
            var gyms = await context.Gym.Where(c => c.city_id == id).ToListAsync();
            return Json(gyms);
        }
        //Список всіх наявних залів, доступ лише у SuperAdmin
        //Також вивід імен власників, а не id через ViewBag
        [Authorize(Roles ="SuperAdmin")]
        public async Task<IActionResult> AllGyms()
        {
            var gyms = await context.Gym.ToListAsync();
            var users = await _userManager.Users.ToDictionaryAsync(u => u.Id, u => u.UserName);
            var ownerNames = new Dictionary<string, string>();
            foreach (var gym in gyms)
            {
                if (!ownerNames.ContainsKey(gym.owner_id!))
                {
                    if (users.TryGetValue(gym.owner_id!, out var ownerName))
                    {
                        ownerNames.Add(gym.owner_id!, ownerName);
                    }
                }
            }
            ViewBag.OwnerNames = ownerNames;
            return View(gyms);
        }
        //Сторінка зі всіма полями на зал.Редагувати може лише власник або SuperAdmin
        //Модель містить сам зал та списки Спорту та Приналежностей з можливістю вибору наявних через чекбокси
        [HttpGet]
        public async Task<IActionResult> EditGym(int id)
        {
            var gym = await context.Gym.FindAsync(id);
            if (gym == null)
            {
                return NotFound();
            }
            var gymAccessories = await context.Accessory
            .Select(a => new GymAccessoryView
            {
                AccessoryId = a.id,
                AccessoryName = a.name,
                IsSelected = context.GymAccessory
                    .Any(ga => ga.gym_id == id && ga.accessory_id == a.id)
            })
            .ToListAsync();
            var gymSports = await context.Sport
            .Select(a => new GymSportView
            {
                SportId = a.id,
                SportName = a.name,
                IsSelected = context.GymSport
                    .Any(ga => ga.gym_id == id && ga.sport_id == a.id)
            })
            .ToListAsync();
            var model = new EditGym
            {
                gym = gym,
                Accessories = gymAccessories,
                Sports = gymSports
            };
            string user = _userManager.GetUserId(User);

            if (!(User.IsInRole("Admin") && gym.owner_id == user || User.IsInRole("SuperAdmin")))
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this gym.";
                return NotFound();
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditGym(EditGym model, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                var gym = await context.Gym.FindAsync(model.gym!.id);
                var gymAccessories = context.GymAccessory.Where(ga => ga.gym_id == model.gym.id);
                var gymAccessoryIds = gymAccessories.Select(ga => ga.accessory_id).ToList();

                var gymSports = context.GymSport.Where(ga => ga.gym_id == model.gym.id);
                var gymSportIds = gymSports.Select(ga => ga.sport_id).ToList();
                if (gym == null)
                {
                    return NotFound();
                }
                // Delete old photo if it's not the default one
                if (!string.IsNullOrEmpty(gym.imagePath) && !gym.imagePath.Equals("/images/default-image.png") && photo != null)
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, gym.imagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // add photo
                if (photo != null && photo.Length > 0)
                {
                    var maxFileSize = 5 * 1024 * 1024; // 5 MB (это уже сам меняешь как тебе надо)
                    if (photo.Length > maxFileSize)
                    {
                        // тут напиши текст ошибки
                        // TempData["ErrorMessage"] = "";
                        return RedirectToAction("AddGym");
                    }
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        // тут напиши текст ошибки
                        // TempData["ErrorMessage"] = "";
                        return RedirectToAction("AddGym");
                    }
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(fileStream);
                    }
                    gym.imagePath = "/images/" + uniqueFileName;
                }
                //else
                //{
                //    gym.imagePath = "/images/default-image.png";
                //}
                // Add new accessories
                foreach (var accessoryId in model.Accessories!)
                {
                    if (!gymAccessories.Any(a =>a.accessory_id==accessoryId.AccessoryId) && accessoryId.IsSelected==true)
                    {
                        context.GymAccessory.Add(new GymAccessory { gym_id = model.gym.id, accessory_id = accessoryId.AccessoryId });
                    }
                }
                // Add new sports
                foreach (var sportId in model.Sports!)
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
                gym.mapLocate = model.gym.mapLocate;
                gym.description = model.gym.description;
                gym.price = model.gym.price;
                gym.startwork = model.gym.startwork;
                gym.endwork = model.gym.endwork;

                await context.SaveChangesAsync();
                TempData["GoodMessage"] = "Gym edited!";
                return RedirectToAction("EditGym", new { gym.id });
            }

            return View(model);
        }
        //Функія для додачі нового залу(тільки адмін або суперадмін)
        //Включені необхідні поля для залу,спорту та принадлежностей
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
        public async Task<IActionResult> AddGym(AddGym gym, List<int> idlis, List<int> idsport, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var newgym = new Gym
                {
                    city_id = gym.city_id,
                    owner_id = user.Id,
                    name = gym.name,
                    mapLocate = gym.mapLocate,
                    description = gym.description,
                    adress = gym.adress,
                    price = gym.price,
                    startwork = gym.startwork,
                    endwork = gym.endwork
                };
                if (user != null)
                {
                    // add photo
                    if (photo != null && photo.Length > 0)
                    {
                        var maxFileSize = 5 * 1024 * 1024; // 5 MB (это уже сам меняешь как тебе надо)
                        if (photo.Length > maxFileSize)
                        {
                            // тут напиши текст ошибки
                            // TempData["ErrorMessage"] = "";
                            return RedirectToAction("AddGym");
                        }
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png"};
                        var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            // тут напиши текст ошибки
                            // TempData["ErrorMessage"] = "";
                            return RedirectToAction("AddGym");
                        }
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(fileStream);
                        }
                        newgym.imagePath = "/images/" + uniqueFileName;
                    }
                    else
                    {
                        newgym.imagePath = "/images/default-image.png";
                    }
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
                    await context.SaveChangesAsync();
                    TempData["GoodMessage"] = $"Added new gym with id - {newgym.id}!";
                    return RedirectToAction("OwnedGyms");
                }

            }
            TempData["ErrorMessage"] = "Invalid data";
            return View(gym);
        }
        //Видалення залу(можливість лише у власника і суперадміна)
        [Authorize]
        public async Task<IActionResult> DeleteGym(int id)
        {
            string user = _userManager.GetUserId(User);
            var gym = await context.Gym.FindAsync(id);
            if (!(User.IsInRole("Admin") && gym!.owner_id == user || User.IsInRole("SuperAdmin")))
            {
                TempData["ErrorMessage"] = "You do not have permission to delete this gym.";
                return NotFound();
            }
            if (gym == null)
            {
                return NotFound();
            }

            if (!gym!.imagePath.Equals("/images/default-image.png"))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, gym.imagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            context.Gym.Remove(gym);
            await context.SaveChangesAsync();

            return RedirectToAction("OwnedGyms");
        }
        //Вивід залів у яких поточний користувач власник
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> OwnedGyms()
        {
            var userId = _userManager.GetUserId(User);

            var gyms = await context.Gym.Where(g => g.owner_id == userId).ToListAsync();
            if (gyms.Count == 0)
            {
                TempData["InfoMessage"] = "You do not have any gyms";
            }

            return View(gyms);
        }
        //Вивід інформації про конкретний зал з переходом на бронювання

        [HttpGet]
        public IActionResult GymEditPrice(int id)
        {
            string user = _userManager.GetUserId(User);
            var gym = context.Gym.Find(id)!;
            if (!(User.IsInRole("Admin") && gym.owner_id == user || User.IsInRole("SuperAdmin")))
            {
                TempData["ErrorMessage"] = "You do not have permission to edit price for this  gym.";
                return NotFound();
            }
            if (gym == null)
            {
                return NotFound();
            }
            var gymSports = context.GymSport
            .Where(ga => ga.gym_id == id)
            .Join(context.Sport,
                ga => ga.sport_id,
                a => a.id,
                (ga, a) => new SportWithPrice { Sport = a, Price = ga.price })
            .ToList();
            ViewBag.SportsWithPrice = gymSports;
            ViewBag.GymId = id;
            return View();
        }
        [HttpPost]
        public IActionResult GymEditPrice(int gymId, List<int> sportId, List<decimal> price)
        {
            string user = _userManager.GetUserId(User);
            var gym = context.Gym.Find(gymId)!;
            if (!(User.IsInRole("Admin") && gym.owner_id == user || User.IsInRole("SuperAdmin")))
            {
                TempData["ErrorMessage"] = "You do not have permission to edit price for this  gym.";
                return NotFound();
            }
            if (gym == null)
            {
                return NotFound();
            }

            for (int i = 0; i < sportId.Count; i++)
            {
                int currentSportId = sportId[i];
                decimal currentPrice = price[i];

                var gymSport = context.GymSport
                    .FirstOrDefault(gs => gs.gym_id == gymId && gs.sport_id == currentSportId);

                if (gymSport != null)
                {
                    if (gymSport.price != currentPrice)
                    {
                        gymSport.price = currentPrice;
                        context.SaveChanges();
                    }

                }
            }
            TempData["GoodMessage"] = "All prices saved!";

            return RedirectToAction("GymEditPrice", new { id = gymId });
        }

        [HttpGet]
        public async Task<IActionResult> GymDetails(int id)
        {
            var gym = await context.Gym.FindAsync(id);

            if (gym == null)
            {
                TempData["ErrorMessage"] = "Gym doesn't exist";
                return NotFound();
            }

            List<Accessory> accessoryNames = await context.GymAccessory
                .Where(ga => ga.gym_id == id)
                .Join(context.Accessory,
                      ga => ga.accessory_id,
                      a => a.id,
                      (ga, a) => a)
                .ToListAsync();
            List<SportWithPrice> sportNames = await context.GymSport
                .Where(ga => ga.gym_id == id)
                .Join(context.Sport,
                    ga => ga.sport_id,
                    a => a.id,
                    (ga, a) => new SportWithPrice { Sport = a, Price = ga.price })
                .ToListAsync();

            var viewModel = new GymView
            {
                Id = id,
                Name = gym.name,
                ImagePath = gym.imagePath,
                MapLocate = gym.mapLocate,
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
        //Бронювання залу на конкретну дату,час,спорт
        [HttpGet]
        public IActionResult BookGym(int gymId, DateTime selectedDate)
        {
            var gym = context.Gym.Find(gymId)!;
            if (selectedDate == default)
            {
                selectedDate = DateTime.Now.Date;
            }
            int startHour = gym.startwork;
            int endHour = gym.endwork;
            //Вивід вільного спорту під конкретний час
            var availableSlots =  Enumerable.Range(gym.startwork, gym.endwork-gym.startwork)
                .Select( hour => new {
                    BookingDate = selectedDate,
                    BookingHour = hour,
                    AvailableSports = context.GymSport
            .Join(context.Sport, gs => gs.sport_id, s => s.id, (gs, s) => new { Sport = s, GymSport = gs })
            .Where(joined => joined.GymSport.gym_id == gymId && !context.BookingOrders.Any(b => b.GymId == gymId && b.BookedSportid == joined.GymSport.sport_id && b.BookingDate == selectedDate && b.BookingHour == hour))
            .Select(joined => new { SportId = joined.Sport.id, SportName = joined.Sport.name, SportPrice = joined.GymSport.price })
            .ToList()
                })
                .ToList();


            ViewBag.AvailableSlots = availableSlots;


            var viewModel = new BookGymViewModel
            {
                GymId = gym.id,
                GymName = gym.name,
                SelectedDate = selectedDate
            };
            return View(viewModel);
        }
        //Бронювання залу на конкретну дату,час,спорт
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BookGym(int GymId, DateTime bookingDate, string selectedSlot, bool addToCalendar)
        {
            if(selectedSlot == null)
            {
                TempData["ErrorMessage"] = "Please select any sport before booking!";
                return RedirectToAction("BookGym", new { gymId = GymId });
            }
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
                BookedSportid = selectedSportId,
                OrderPrice = await context.GymSport
                    .Where(gs => gs.gym_id == GymId && gs.sport_id == selectedSportId)
                    .Select(gs => gs.price)
                    .FirstOrDefaultAsync()
            };

            await context.BookingOrders.AddAsync(bookingOrder);
            await context.SaveChangesAsync();

            if (addToCalendar) {
                await _gCalendar.AddEventToGoogleCalendarAsync(bookingOrder,
                    await context.Gym
                        .Where(g => g.id == GymId)
                        .FirstAsync(),
                    await context.Sport
                        .Where(s => s.id == selectedSportId)
                        .Select(s => s.name)
                        .FirstAsync()
                    );
            }

            TempData["GoodMessage"] = $"Successfully booked!Booking order id is {bookingOrder.Id}";
            return RedirectToAction("BookGym", new { gymId = GymId });
        }
        //Вивід таблиці замовлень
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> OrderedGyms()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await context.BookingOrders
                    .Where(g => g.UserId == userId)
                    .OrderByDescending(g => g.Id)
                    .ToListAsync();
            if (orders.Count == 0)
            {
                TempData["InfoMessage"] = "You do not have any orders";
            }
            return View(orders);
        }
    }
}
