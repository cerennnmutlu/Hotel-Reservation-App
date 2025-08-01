using HotelReservationApp.Data;
using HotelReservationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HotelReservationApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly HotelReservationContext _context;

        public AdminController(HotelReservationContext context)
        {
            _context = context;
        }

        // Admin paneli ana sayfası
        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalHotels = await _context.Hotels.CountAsync();
            var totalReservations = await _context.Reservations.CountAsync();
            var totalReviews = await _context.Reviews.CountAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalHotels = totalHotels;
            ViewBag.TotalReservations = totalReservations;
            ViewBag.TotalReviews = totalReviews;

            return View();
        }

        // Kullanıcıları listeleme
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // Otelleri listeleme
        public async Task<IActionResult> Hotels()
        {
            var hotels = await _context.Hotels
                .Include(h => h.Rooms)
                .ToListAsync();
            return View(hotels);
        }

        // Yorumları listeleme
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Hotel)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
            return View(reviews);
        }

        // Rezervasyonları listeleme
        public async Task<IActionResult> Reservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Room)
                .ThenInclude(r => r.Hotel)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
            return View(reservations);
        }

        // Kullanıcı oluşturma
        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                user.CreatedDate = DateTime.Now;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(Users));
            }

            TempData["ErrorMessage"] = "Failed to create user. Please check the form.";
            return RedirectToAction(nameof(Users));
        }

        // Kullanıcı silme
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "User deleted successfully!";
            return RedirectToAction(nameof(Users));
        }

        // Kullanıcı düzenleme
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == id);
            
            if (user == null)
                return NotFound();

            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction(nameof(Users));
            }

            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(user);
        }

        // Otel oluşturma
        [HttpPost]
        public async Task<IActionResult> CreateHotel(Hotel hotel)
        {
            if (ModelState.IsValid)
            {
                _context.Hotels.Add(hotel);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hotel created successfully!";
                return RedirectToAction(nameof(Hotels));
            }

            TempData["ErrorMessage"] = "Failed to create hotel. Please check the form.";
            return RedirectToAction(nameof(Hotels));
        }

        // Otel silme
        [HttpPost]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
                return NotFound();

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Hotel deleted successfully!";
            return RedirectToAction(nameof(Hotels));
        }

        // Otel düzenleme
        public async Task<IActionResult> EditHotel(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.City)
                .FirstOrDefaultAsync(h => h.HotelID == id);
            
            if (hotel == null)
                return NotFound();

            ViewBag.Cities = await _context.Cities.ToListAsync();
            return View(hotel);
        }

        [HttpPost]
        public async Task<IActionResult> EditHotel(Hotel hotel)
        {
            if (ModelState.IsValid)
            {
                _context.Update(hotel);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hotel updated successfully!";
                return RedirectToAction(nameof(Hotels));
            }

            ViewBag.Cities = await _context.Cities.ToListAsync();
            return View(hotel);
        }

        // Rezervasyon durumu güncelleme
        [HttpPost]
        public async Task<IActionResult> UpdateReservationStatus(int id, string status)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            reservation.Status = status;
            reservation.UpdatedDate = DateTime.Now;
            
            _context.Update(reservation);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Reservation status updated successfully!";
            return RedirectToAction(nameof(Reservations));
        }

        // Yorum silme
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Review deleted successfully!";
            return RedirectToAction(nameof(Reviews));
        }

        // Review detayları için AJAX endpoint
        public async Task<IActionResult> GetReviewDetails(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
                .FirstOrDefaultAsync(r => r.ReviewID == id);

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

        // Dashboard istatistikleri
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalHotels = await _context.Hotels.CountAsync(),
                TotalReservations = await _context.Reservations.CountAsync(),
                TotalReviews = await _context.Reviews.CountAsync(),
                RecentReservations = await _context.Reservations
                    .Include(r => r.User)
                    .Include(r => r.Room)
                    .OrderByDescending(r => r.CreatedDate)
                    .Take(5)
                    .ToListAsync()
            };

            return Json(stats);
        }
    }
}
