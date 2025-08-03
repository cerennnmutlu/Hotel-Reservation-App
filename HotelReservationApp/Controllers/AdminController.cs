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
            var users = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
            return View(users);
        }

        // Müşterileri listeleme
        public async Task<IActionResult> Customers()
        {
            var customers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName == "Customer")
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
            return View(customers);
        }

        // Hotel Manager'ları listeleme
        public async Task<IActionResult> HotelManagers()
        {
            var managers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName == "Hotel Manager")
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
            return View(managers);
        }

        // Admin'leri listeleme
        public async Task<IActionResult> Admins()
        {
            var admins = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName == "Admin")
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
            return View(admins);
        }

        // Kullanıcı detaylarını görüntüleme
        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Reservations)
                .ThenInclude(r => r.Room)
                .ThenInclude(r => r.Hotel)
                .Include(u => u.Reviews)
                .ThenInclude(r => r.Hotel)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
                return Json(new { success = false, message = "User not found." });

            return PartialView("_UserDetails", user);
        }

        // Kullanıcı oluşturma sayfası
        public async Task<IActionResult> CreateUser()
        {
            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View();
        }

        // Kullanıcı oluşturma
        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                // Email kontrolü
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "This email is already registered." });
                }

                user.CreatedDate = DateTime.Now;
                // Şifre doğrudan PasswordHash alanına kaydediliyor
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "User created successfully!" });
            }

            return Json(new { success = false, message = "Failed to create user. Please check the form." });
        }

        // Kullanıcı düzenleme sayfası
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

        // Kullanıcı düzenleme
        [HttpPost]
        public async Task<IActionResult> EditUser(User user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FindAsync(user.UserID);
                if (existingUser == null)
                    return Json(new { success = false, message = "User not found." });

                // Email kontrolü (kendi email'i hariç)
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == user.Email && u.UserID != user.UserID);
                if (emailExists)
                {
                    return Json(new { success = false, message = "This email is already registered." });
                }

                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.RoleID = user.RoleID;
                existingUser.IsActive = user.IsActive;

                // Şifre değiştirilmişse güncelle
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    existingUser.PasswordHash = user.PasswordHash;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "User updated successfully!" });
            }

            return Json(new { success = false, message = "Failed to update user. Please check the form." });
        }

        // Kullanıcı silme
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Reservations)
                .Include(u => u.Reviews)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
                return Json(new { success = false, message = "User not found." });

            // Kullanıcının rezervasyonları veya yorumları varsa silme
            if (user.Reservations.Any() || user.Reviews.Any())
            {
                return Json(new { success = false, message = "Cannot delete user with existing reservations or reviews." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "User deleted successfully!" });
        }

        // Kullanıcı durumunu değiştirme (aktif/pasif)
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            var status = user.IsActive ? "activated" : "deactivated";
            return Json(new { success = true, message = $"User {status} successfully!" });
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

        // Otel odalarını listeleme
        public async Task<IActionResult> HotelRooms(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomType)
                .FirstOrDefaultAsync(h => h.HotelID == id);
            
            if (hotel == null)
                return NotFound();

            ViewBag.HotelName = hotel.Name;
            return View(hotel.Rooms);
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

        // Yorum detaylarını getir
        public async Task<IActionResult> GetReviewDetails(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Hotel)
                .ThenInclude(h => h.City)
                .FirstOrDefaultAsync(r => r.ReviewID == id);

            if (review == null)
                return NotFound();

            var reviewDetails = new
            {
                reviewId = review.ReviewID,
                userName = review.User?.FullName,
                userEmail = review.User?.Email,
                hotelName = review.Hotel?.Name,
                hotelCity = review.Hotel?.City?.CityName,
                rating = review.Rating,
                comment = review.Comment,
                reviewDate = review.ReviewDate?.ToString("MMM dd, yyyy")
            };

            return Json(reviewDetails);
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
                    .ThenInclude(room => room.Hotel)
                    .OrderByDescending(r => r.CreatedDate)
                    .Take(5)
                    .Select(r => new {
                        r.ReservationID,
                        r.Status,
                        r.CreatedDate,
                        User = new {
                            r.User.FullName,
                            r.User.Email
                        },
                        Room = new {
                            r.Room.RoomNumber,
                            Hotel = new {
                                r.Room.Hotel.Name
                            }
                        }
                    })
                    .ToListAsync()
            };

            return Json(stats);
        }

        // AJAX için kullanıcı verilerini getir
        public async Task<IActionResult> GetUsersData()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedDate)
                .Select(u => new
                {
                    userID = u.UserID,
                    fullName = u.FullName,
                    email = u.Email,
                    phone = u.Phone,
                    roleID = u.RoleID,
                    roleName = u.Role.RoleName,
                    isActive = u.IsActive,
                    createdDate = u.CreatedDate
                })
                .ToListAsync();

            return Json(users);
        }
    }
}
