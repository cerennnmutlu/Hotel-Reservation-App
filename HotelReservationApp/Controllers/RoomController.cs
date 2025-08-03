using HotelReservationApp.Data;
using HotelReservationApp.Models; // DbContext ve Room model için
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationApp.Controllers
{
    public class RoomController : Controller
    {
        private readonly HotelReservationContext _context;

        public RoomController(HotelReservationContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _context
                .Rooms.Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .Include(r => r.RoomImages)
                .ToListAsync();
            return View(rooms);
        }

        public async Task<IActionResult> Details(int id)
        {
            var room = await _context
                .Rooms.Include(r => r.Hotel)
                .ThenInclude(h => h.HotelAmenityMappings)
                .ThenInclude(m => m.Amenity)
                .Include(r => r.RoomType)
                .Include(r => r.RoomImages)
                .FirstOrDefaultAsync(r => r.RoomID == id);

            if (room == null)
                return NotFound();

            var availabilityList = await _context
                .RoomAvailabilities.Where(ra => ra.RoomID == id)
                .OrderBy(ra => ra.Date)
                .ToListAsync();

            ViewBag.AvailabilityList = availabilityList;

            return PartialView("_RoomDetailsPartial", room);
        }

        public IActionResult Create()
        {
            ViewBag.Hotels = _context.Hotels.ToList();
            ViewBag.RoomTypes = _context.Set<RoomType>().ToList();
            return PartialView("_RoomFormPartial", new Room());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return PartialView("_RoomFormPartial", room);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();
            ViewBag.Hotels = _context.Hotels.ToList();
            ViewBag.RoomTypes = _context.Set<RoomType>().ToList();
            return PartialView("_RoomFormPartial", room);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Update(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return PartialView("_RoomFormPartial", room);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Search rooms based on booking criteria
        public async Task<IActionResult> Search(BookingViewModel searchModel)
        {
            var query = _context.Rooms
                .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
                .Include(r => r.Hotel)
                .ThenInclude(h => h.Reviews)
                .Include(r => r.RoomType)
                .Include(r => r.RoomImages)
                .AsQueryable();

            // Apply filters
            if (searchModel.HotelID.HasValue)
            {
                query = query.Where(r => r.HotelID == searchModel.HotelID);
            }

            if (searchModel.CityID.HasValue)
            {
                query = query.Where(r => r.Hotel.CityID == searchModel.CityID);
            }

            if (searchModel.RoomTypeID.HasValue)
            {
                query = query.Where(r => r.RoomTypeID == searchModel.RoomTypeID);
            }

            // Filter by capacity (adults + children)
            var totalGuests = searchModel.AdultCount + searchModel.ChildCount;
            query = query.Where(r => r.Capacity >= totalGuests && r.Capacity <= totalGuests + 1);

            // Date availability check (if dates are provided)
            if (!string.IsNullOrEmpty(searchModel.CheckInDate) && !string.IsNullOrEmpty(searchModel.CheckOutDate))
            {
                if (DateTime.TryParse(searchModel.CheckInDate, out DateTime checkIn) && 
                    DateTime.TryParse(searchModel.CheckOutDate, out DateTime checkOut))
                {
                    // Get rooms that are available for the specified date range
                    var unavailableRoomIds = await _context.RoomAvailabilities
                        .Where(ra => ra.Date >= checkIn && ra.Date < checkOut && ra.IsAvailable == false)
                        .Select(ra => ra.RoomID)
                        .Distinct()
                        .ToListAsync();

                    query = query.Where(r => !unavailableRoomIds.Contains(r.RoomID));
                }
            }

            var availableRooms = await query.ToListAsync();

            // Pass search criteria to view for display
            ViewBag.SearchCriteria = searchModel;
            ViewBag.TotalResults = availableRooms.Count;

            return View("Search", availableRooms);
        }
    }
}
