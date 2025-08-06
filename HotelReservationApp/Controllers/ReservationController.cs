using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using HotelReservationApp.Data;
using HotelReservationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;

namespace HotelReservationApp.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly HotelReservationContext _context;

        public ReservationController(HotelReservationContext context)
        {
            _context = context;
        }

        // All reservations - Only Admin and Manager can see
        [Authorize(Roles = "Admin,Hotel Manager")]
        public async Task<IActionResult> Index()
        {
            var reservations = await _context
                .Reservations.Include(r => r.User)
                .Include(r => r.Room)
                .ThenInclude(room => room.Hotel)
                .Include(r => r.Room.RoomImages)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(reservations);
        }

        // User's own reservations
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyReservations()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var reservations = await _context
                .Reservations.Where(r => r.UserID == userId)
                .Include(r => r.Room)
                .ThenInclude(room => room.Hotel)
                .Include(r => r.Room.RoomImages)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(reservations);
        }

        // Create reservation (customer)
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(int roomId, DateTime? checkIn = null, DateTime? checkOut = null, int? guestCount = null)
        {
            var room = await _context
                .Rooms.Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .Include(r => r.RoomImages)
                .FirstOrDefaultAsync(r => r.RoomID == roomId);

            if (room == null)
                return NotFound();

            var viewModel = new ReservationViewModel
            {
                RoomID = roomId,
                CheckInDate = checkIn ?? DateTime.Today.AddDays(1),
                CheckOutDate = checkOut ?? DateTime.Today.AddDays(2),
                GuestCount = guestCount ?? 1,
            };

            ViewBag.Room = room;
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(ReservationViewModel viewModel)
        {
            // Hata ayıklama için rezervasyon verilerini logla
            Console.WriteLine($"Received reservation: Room={viewModel?.RoomID}, CheckIn={viewModel?.CheckInDate}, CheckOut={viewModel?.CheckOutDate}, Guests={viewModel?.GuestCount}");
            
            // Request body'yi kontrol et
            try
            {
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    Request.Body.Position = 0; // Stream'i başa sar
                    var bodyContent = reader.ReadToEndAsync().Result;
                    Console.WriteLine($"Request body: {bodyContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading request body: {ex.Message}");
            }
            
            // ViewModel null kontrolü
            if (viewModel == null)
            {
                Console.WriteLine("Reservation viewModel is null");
                return Json(new { success = false, message = "Rezervasyon verileri alınamadı." });
            }
            
            // Model durumunu kontrol et
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                Console.WriteLine($"Model key: {key}, Valid: {state.ValidationState}, Value: {state.RawValue}");
                
                if (state.Errors.Any())
                {
                    Console.WriteLine($"Errors for {key}: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            
            // Temel model doğrulama kontrolü
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                             .Select(e => e.ErrorMessage)
                                             .ToList();
                Console.WriteLine($"Model validation errors: {string.Join(", ", errors)}");
                return Json(new { success = false, errors = errors });
            }
            
            // ViewModel'i Reservation modeline dönüştür
            var reservation = viewModel.ToReservation();
            
            try
            {
                // Tüm form verilerini logla
                Console.WriteLine("Tüm form verileri:");
                foreach (var key in Request.Form.Keys)
                {
                    Console.WriteLine($"{key}: {Request.Form[key]}");
                }
                
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    Console.WriteLine("User ID claim not found or invalid");
                    return Json(new { success = false, message = "Kullanıcı girişi gerekli. Lütfen tekrar giriş yapın." });
                }
                
                Console.WriteLine($"User ID: {userId}");
                
                // Rezervasyon verilerini manuel olarak ayarla
                if (reservation.RoomID <= 0 && Request.Form.ContainsKey("RoomID") && int.TryParse(Request.Form["RoomID"], out int roomId))
                {
                    Console.WriteLine($"Manuel olarak RoomID ayarlanıyor: {roomId}");
                    reservation.RoomID = roomId;
                }
                
                if (Request.Form.ContainsKey("CheckInDate") && DateTime.TryParse(Request.Form["CheckInDate"], out DateTime checkInDate))
                {
                    Console.WriteLine($"Manuel olarak CheckInDate ayarlanıyor: {checkInDate}");
                    reservation.CheckInDate = checkInDate;
                }
                
                if (Request.Form.ContainsKey("CheckOutDate") && DateTime.TryParse(Request.Form["CheckOutDate"], out DateTime checkOutDate))
                {
                    Console.WriteLine($"Manuel olarak CheckOutDate ayarlanıyor: {checkOutDate}");
                    reservation.CheckOutDate = checkOutDate;
                }
                
                if (reservation.GuestCount <= 0 && Request.Form.ContainsKey("GuestCount") && int.TryParse(Request.Form["GuestCount"], out int guestCount))
                {
                    Console.WriteLine($"Manuel olarak GuestCount ayarlanıyor: {guestCount}");
                    reservation.GuestCount = guestCount;
                }
                
                if (Request.Form.ContainsKey("SpecialRequests"))
                {
                    Console.WriteLine($"Manuel olarak SpecialRequests ayarlanıyor: {Request.Form["SpecialRequests"]}");
                    reservation.SpecialRequests = Request.Form["SpecialRequests"];
                }

                // Güncellenmiş rezervasyon verilerini logla
                Console.WriteLine($"Güncellenmiş rezervasyon: Room={reservation.RoomID}, CheckIn={reservation.CheckInDate}, CheckOut={reservation.CheckOutDate}, Guests={reservation.GuestCount}");
                
                // Validate dates
                if (reservation.CheckInDate >= reservation.CheckOutDate)
                {
                    Console.WriteLine("Invalid dates: CheckOut must be after CheckIn");
                    return Json(new { success = false, message = "Çıkış tarihi, giriş tarihinden sonra olmalıdır." });
                }

                if (reservation.CheckInDate < DateTime.Today)
                {
                    Console.WriteLine("Invalid dates: CheckIn cannot be in the past");
                    return Json(new { success = false, message = "Giriş tarihi geçmiş bir tarih olamaz." });
                }
                
                // Oda varlığını kontrol et
                var room = await _context.Rooms.FindAsync(reservation.RoomID);
                if (room == null)
                {
                    Console.WriteLine($"Room with ID {reservation.RoomID} not found");
                    return Json(new { success = false, message = "Belirtilen oda bulunamadı." });
                }

                // Check room availability
                var conflictingReservations = await _context.Reservations
                    .Where(r => r.RoomID == reservation.RoomID && 
                               r.Status != "Cancelled" &&
                               ((r.CheckInDate <= reservation.CheckInDate && r.CheckOutDate > reservation.CheckInDate) ||
                                (r.CheckInDate < reservation.CheckOutDate && r.CheckOutDate >= reservation.CheckOutDate) ||
                                (r.CheckInDate >= reservation.CheckInDate && r.CheckOutDate <= reservation.CheckOutDate)))
                    .ToListAsync();

                if (conflictingReservations.Any())
                {
                    Console.WriteLine($"Found {conflictingReservations.Count} conflicting reservations");
                    return Json(new { success = false, message = "Seçilen tarihler için oda müsait değil. Lütfen başka tarih seçin." });
                }

                // Rezervasyon bilgilerini ayarla
            reservation.UserID = userId;
            
            // Eğer client tarafından gönderilmediyse varsayılan değerleri ayarla
            if (string.IsNullOrEmpty(reservation.Status))
            {
                reservation.Status = "Confirmed";
            }
            
            if (reservation.CreatedDate == default(DateTime))
            {
                reservation.CreatedDate = DateTime.Now;
            }
                
                // Calculate total amount
                reservation.TotalAmount = await CalculateTotalAmount(
                    reservation.RoomID,
                    reservation.CheckInDate,
                    reservation.CheckOutDate
                );
                
                Console.WriteLine($"Calculated total amount: {reservation.TotalAmount}");

                // Veritabanına kaydet
                try {
                    _context.Reservations.Add(reservation);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"Reservation created successfully with ID: {reservation.ReservationID}");
    
                    return Json(new { 
                        success = true, 
                        message = "Rezervasyonunuz başarıyla oluşturuldu.",
                        redirectTo = Url.Action(nameof(MyReservations)),
                        reservationId = reservation.ReservationID
                    });
                } catch (Exception dbEx) {
                    Console.WriteLine($"Database error: {dbEx.Message}");
                    Console.WriteLine($"Database error stack trace: {dbEx.StackTrace}");
                    return Json(new { success = false, message = "Veritabanı hatası: " + dbEx.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating reservation: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Rezervasyon oluşturulurken bir hata oluştu: " + ex.Message });
            }
        }

        // Details (Customer)
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Details(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var reservation = await _context
                .Reservations.Include(r => r.User)
                .Include(r => r.Room)
                .ThenInclude(room => room.Hotel)
                .ThenInclude(hotel => hotel.City)
                .Include(r => r.Room.RoomType)
                .Include(r => r.Room.RoomImages)
                .FirstOrDefaultAsync(r => r.ReservationID == id && r.UserID == userId);

            if (reservation == null)
                return NotFound();

            // Get reviews for this hotel from this user's reservations
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.HotelID == reservation.Room.HotelID)
                .Select(r => new ReviewViewModel
                {
                    ReviewID = r.ReviewID,
                    UserID = r.UserID,
                    UserName = r.User.FullName,
                    HotelID = r.HotelID,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate,
                    ReservationID = id // Associate with current reservation for display
                })
                .ToListAsync();

            // Check if user can add review (reservation is confirmed and completed)
            var canAddReview = reservation.Status == "Confirmed" && 
                              reservation.CheckOutDate < DateTime.Now;

            // Check if user has already reviewed this hotel
            var hasUserReviewed = await _context.Reviews
                .AnyAsync(r => r.HotelID == reservation.Room.HotelID && r.UserID == userId);

            var viewModel = new ReservationDetailsViewModel
            {
                Reservation = reservation,
                Reviews = reviews,
                CanAddReview = canAddReview,
                HasUserReviewed = hasUserReviewed,
                CurrentUserId = userId.ToString()
            };

            return View(viewModel);
        }

        // Details (Admin & Manager)
        [Authorize(Roles = "Admin,Hotel Manager")]
        public async Task<IActionResult> AdminDetails(int id)
        {
            var reservation = await _context
            .Reservations.Include(r => r.User)
            .Include(r => r.Room)
            .ThenInclude(room => room.Hotel)
            .ThenInclude(hotel => hotel.City)
            .Include(r => r.Room.RoomType)
            .Include(r => r.Room.RoomImages)
            .FirstOrDefaultAsync(r => r.ReservationID == id);

            if (reservation == null)
                return NotFound();

            return View("Details", reservation);
        }

        // Add Review
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddReview(AddReviewViewModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            // Check if reservation belongs to user
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .ThenInclude(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.ReservationID == model.ReservationID && r.UserID == userId);

            if (reservation == null)
            {
                return Json(new { success = false, message = "Rezervasyon bulunamadı." });
            }

            // Check if user already reviewed this hotel
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.HotelID == model.HotelID && r.UserID == userId);

            if (existingReview != null)
            {
                return Json(new { success = false, message = "Bu otel için zaten değerlendirme yaptınız" });
            }

            var review = new Review
            {
                UserID = userId,
                HotelID = model.HotelID,
                Rating = model.Rating,
                Comment = model.Comment,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Yorumunuz başarıyla eklendi." });
        }

        // Cancel reservation (customer)
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            // Get user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user owns this reservation or is admin/manager
            if (reservation.UserID != userId && !User.IsInRole("Admin") && !User.IsInRole("Hotel Manager"))
            {
                return Unauthorized();
            }

            // Check if reservation can be cancelled (not too close to check-in)
            if (reservation.CheckInDate <= DateTime.Today.AddDays(1))
            {
                TempData["ErrorMessage"] = "Reservations can only be cancelled at least 24 hours before check-in.";
                return RedirectToAction(nameof(MyReservations));
            }

            reservation.Status = "Cancelled";
            reservation.CancellationDate = DateTime.Now;
            reservation.UpdatedDate = DateTime.Now;

            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation cancelled successfully!";
            return RedirectToAction(nameof(MyReservations));
        }

        // Helper method: Calculate total amount
        private async Task<decimal?> CalculateTotalAmount(
            int roomId,
            DateTime checkIn,
            DateTime checkOut
        )
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
                return null;

            int totalDays = (checkOut - checkIn).Days;
            if (totalDays <= 0)
                return null;

            return totalDays * room.PricePerNight;
        }
    }
}
