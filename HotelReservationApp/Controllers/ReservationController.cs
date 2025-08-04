using System;
using System.Linq;
using System.Threading.Tasks;
using HotelReservationApp.Data;
using HotelReservationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        public async Task<IActionResult> Create(int roomId, DateTime? checkIn = null, DateTime? checkOut = null, int? guestCount = null)
        {
            var room = await _context
                .Rooms.Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .Include(r => r.RoomImages)
                .FirstOrDefaultAsync(r => r.RoomID == roomId);

            if (room == null)
                return NotFound();

            var reservation = new Reservation
            {
                RoomID = roomId,
                CheckInDate = checkIn ?? DateTime.Today.AddDays(1),
                CheckOutDate = checkOut ?? DateTime.Today.AddDays(2),
                GuestCount = guestCount ?? 1,
            };

            ViewBag.Room = room;
            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            // Hata ayıklama için rezervasyon verilerini logla
            Console.WriteLine($"Received reservation: Room={reservation.RoomID}, CheckIn={reservation.CheckInDate}, CheckOut={reservation.CheckOutDate}, Guests={reservation.GuestCount}");
            
            // Temel model doğrulama kontrolü
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                             .Select(e => e.ErrorMessage)
                                             .ToList();
                Console.WriteLine($"Model validation errors: {string.Join(", ", errors)}");
                return Json(new { success = false, errors = errors });
            }
            
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    Console.WriteLine("User ID claim not found or invalid");
                    return Json(new { success = false, message = "Kullanıcı girişi gerekli. Lütfen tekrar giriş yapın." });
                }
                
                Console.WriteLine($"User ID: {userId}");

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
                reservation.CreatedDate = DateTime.Now;
                reservation.Status = "Confirmed";
                
                // Calculate total amount
                reservation.TotalAmount = await CalculateTotalAmount(
                    reservation.RoomID,
                    reservation.CheckInDate,
                    reservation.CheckOutDate
                );
                
                Console.WriteLine($"Calculated total amount: {reservation.TotalAmount}");

                // Veritabanına kaydet
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Reservation created successfully with ID: {reservation.ReservationID}");

                return Json(new { 
                    success = true, 
                    message = "Rezervasyonunuz başarıyla oluşturuldu.",
                    redirectTo = Url.Action(nameof(MyReservations)),
                    reservationId = reservation.ReservationID
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating reservation: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Rezervasyon oluşturulurken bir hata oluştu: " + ex.Message });
            }
        }
        }

        // Details (Admin & Manager)
        [Authorize(Roles = "Admin,Hotel Manager")]
        public async Task<IActionResult> Details(int id)
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

            return View(reservation);
        }

        // Cancel reservation (customer)
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
