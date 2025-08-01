using HotelReservationApp.Data;
using HotelReservationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservationApp.Controllers
{
    [Authorize(Roles = "Hotel Manager")]
    public class HotelManagerController : Controller
    {
        private readonly HotelReservationContext _context;

        public HotelManagerController(HotelReservationContext context)
        {
            _context = context;
        }

        // Hotel Manager Dashboard
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Get hotels managed by this user
            var userHotels = await _context.Hotels
                .Include(h => h.City)
                .Where(h => h.OwnerID == userId)
                .ToListAsync();

            var totalRooms = await _context.Rooms
                .Where(r => userHotels.Select(h => h.HotelID).Contains(r.HotelID))
                .CountAsync();

            var totalReservations = await _context.Reservations
                .Include(r => r.Room)
                .Where(r => userHotels.Select(h => h.HotelID).Contains(r.Room.HotelID))
                .CountAsync();

            var totalReviews = await _context.Reviews
                .Where(r => userHotels.Select(h => h.HotelID).Contains(r.HotelID))
                .CountAsync();

            ViewBag.TotalHotels = userHotels.Count;
            ViewBag.TotalRooms = totalRooms;
            ViewBag.TotalReservations = totalReservations;
            ViewBag.TotalReviews = totalReviews;
            ViewBag.UserHotels = userHotels;

            return View();
        }

        // My Hotels
        public async Task<IActionResult> MyHotels()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var hotels = await _context.Hotels
                .Include(h => h.City)
                .Include(h => h.Rooms)
                .Where(h => h.OwnerID == userId)
                .ToListAsync();

            return View(hotels);
        }

        // My Reservations
        public async Task<IActionResult> MyReservations()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Room)
                .ThenInclude(r => r.Hotel)
                .Where(r => r.Room.Hotel.OwnerID == userId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(reservations);
        }

        // My Reviews
        public async Task<IActionResult> MyReviews()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Hotel)
                .Where(r => r.Hotel.OwnerID == userId)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            return View(reviews);
        }

        // My Rooms
        public async Task<IActionResult> MyRooms()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var rooms = await _context.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .Where(r => r.Hotel.OwnerID == userId)
                .ToListAsync();

            return View(rooms);
        }

        // Edit Hotel
        public async Task<IActionResult> EditHotel(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var hotel = await _context.Hotels
                .Include(h => h.City)
                .FirstOrDefaultAsync(h => h.HotelID == id && h.OwnerID == userId);
            
            if (hotel == null)
                return NotFound();

            ViewBag.Cities = await _context.Cities.ToListAsync();
            return View(hotel);
        }

        [HttpPost]
        public async Task<IActionResult> EditHotel(Hotel hotel)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Ensure user owns this hotel
            var existingHotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.HotelID == hotel.HotelID && h.OwnerID == userId);
            
            if (existingHotel == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                existingHotel.Name = hotel.Name;
                existingHotel.CityID = hotel.CityID;
                existingHotel.Address = hotel.Address;
                existingHotel.Phone = hotel.Phone;
                existingHotel.Email = hotel.Email;
                existingHotel.Description = hotel.Description;

                _context.Update(existingHotel);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hotel updated successfully!";
                return RedirectToAction(nameof(MyHotels));
            }

            ViewBag.Cities = await _context.Cities.ToListAsync();
            return View(hotel);
        }

        // Update Reservation Status
        [HttpPost]
        public async Task<IActionResult> UpdateReservationStatus(int id, string status)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .ThenInclude(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.ReservationID == id && r.Room.Hotel.OwnerID == userId);

            if (reservation == null)
                return NotFound();

            reservation.Status = status;
            reservation.UpdatedDate = DateTime.Now;
            
            _context.Update(reservation);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Reservation status updated successfully!";
            return RedirectToAction(nameof(MyReservations));
        }

        // Delete Review
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var review = await _context.Reviews
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.ReviewID == id && r.Hotel.OwnerID == userId);

            if (review == null)
                return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Review deleted successfully!";
            return RedirectToAction(nameof(MyReviews));
        }

        // Get Review Details
        public async Task<IActionResult> GetReviewDetails(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
                .FirstOrDefaultAsync(r => r.ReviewID == id && r.Hotel.OwnerID == userId);

            if (review == null)
                return NotFound();

            var reviewData = new
            {
                userName = review.User?.FullName,
                userEmail = review.User?.Email,
                hotelName = review.Hotel?.Name,
                hotelCity = review.Hotel?.City?.CityName,
                rating = review.Rating,
                comment = review.Comment,
                reviewDate = review.ReviewDate?.ToString("yyyy-MM-dd") ?? ""
            };

            return Json(reviewData);
        }

        // Get Dashboard Stats
        public async Task<IActionResult> GetDashboardStats()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var userHotels = await _context.Hotels
                .Where(h => h.OwnerID == userId)
                .Select(h => h.HotelID)
                .ToListAsync();

            var stats = new
            {
                TotalHotels = userHotels.Count,
                TotalRooms = await _context.Rooms.CountAsync(r => userHotels.Contains(r.HotelID)),
                TotalReservations = await _context.Reservations
                    .Include(r => r.Room)
                    .CountAsync(r => userHotels.Contains(r.Room.HotelID)),
                TotalReviews = await _context.Reviews.CountAsync(r => userHotels.Contains(r.HotelID)),
                RecentReservations = await _context.Reservations
                    .Include(r => r.User)
                    .Include(r => r.Room)
                    .Where(r => userHotels.Contains(r.Room.HotelID))
                    .OrderByDescending(r => r.CreatedDate)
                    .Take(5)
                    .ToListAsync()
            };

            return Json(stats);
        }
    }
} 