using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservationApp.Data;
using HotelReservationApp.Models;
using System.Threading.Tasks;

namespace HotelReservationApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly HotelReservationContext _context;

        public HomeController(HotelReservationContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var homeViewModel = new HomeViewModel
            {
                TotalRooms = await _context.Rooms.CountAsync(),
                TotalHotels = await _context.Hotels.CountAsync(),
                TotalCustomers = await _context.Users.CountAsync(u => u.Role.RoleName == "Customer"),
                FeaturedRooms = await _context.Rooms
                    .Include(r => r.RoomType)
                    .Include(r => r.Hotel)
                        .ThenInclude(h => h.Reviews)
                    .Include(r => r.RoomImages)
                    .Take(6)
                    .ToListAsync()
            };

            return View(homeViewModel);
        }

        public async Task<IActionResult> About()
        {
            var homeViewModel = new HomeViewModel
            {
                TotalRooms = await _context.Rooms.CountAsync(),
                TotalHotels = await _context.Hotels.CountAsync(),
                TotalCustomers = await _context.Users.CountAsync(u => u.Role.RoleName == "Customer")
            };

            return View(homeViewModel);
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        // GET: Booking form
        [HttpGet]
        public async Task<IActionResult> Booking()
        {
            var bookingViewModel = new BookingViewModel
            {
                CheckInDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
                CheckOutDate = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd")
            };

            ViewBag.Cities = await _context.Cities.ToListAsync();
            ViewBag.Hotels = await _context.Hotels.ToListAsync();
            ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();

            return View(bookingViewModel);
        }

        // POST: Booking search
        [HttpPost]
        public async Task<IActionResult> Booking(BookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate dates
                if (DateTime.TryParse(model.CheckInDate, out DateTime checkIn) && 
                    DateTime.TryParse(model.CheckOutDate, out DateTime checkOut))
                {
                    if (checkIn >= checkOut)
                    {
                        ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date.");
                        ViewBag.Cities = await _context.Cities.ToListAsync();
                        ViewBag.Hotels = await _context.Hotels.ToListAsync();
                        ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();
                        return View(model);
                    }

                    if (checkIn < DateTime.Today)
                    {
                        ModelState.AddModelError("CheckInDate", "Check-in date cannot be in the past.");
                        ViewBag.Cities = await _context.Cities.ToListAsync();
                        ViewBag.Hotels = await _context.Hotels.ToListAsync();
                        ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();
                        return View(model);
                    }
                }

                // Redirect to room search with the booking criteria
                return RedirectToAction("Search", "Room", model);
            }

            ViewBag.Cities = await _context.Cities.ToListAsync();
            ViewBag.Hotels = await _context.Hotels.ToListAsync();
            ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();

            return View(model);
        }
    }
}
