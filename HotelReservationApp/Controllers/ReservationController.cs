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
        public async Task<IActionResult> Create([FromBody] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get user ID from claims
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    {
                        return Json(new { success = false, message = "Kullanıcı girişi gerekli." });
                    }

                    // Validate dates
                    if (reservation.CheckInDate >= reservation.CheckOutDate)
                    {
                        return Json(new { success = false, message = "Çıkış tarihi, giriş tarihinden sonra olmalıdır." });
                    }

                    if (reservation.CheckInDate < DateTime.Today)
                    {
                        return Json(new { success = false, message = "Giriş tarihi geçmiş bir tarih olamaz." });
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
                        return Json(new { success = false, message = "Seçilen tarihler için oda müsait değil." });
                    }

                    reservation.UserID = userId;
                    reservation.CreatedDate = DateTime.Now;
                    reservation.Status = "Confirmed";
                    
                    // Calculate total amount
                    reservation.TotalAmount = await CalculateTotalAmount(
                        reservation.RoomID,
                        reservation.CheckInDate,
                        reservation.CheckOutDate
                    );

                    _context.Reservations.Add(reservation);
                    await _context.SaveChangesAsync();

                    return Json(new { 
                        success = true, 
                        message = "Rezervasyonunuz başarıyla oluşturuldu.",
                        redirectTo = Url.Action(nameof(MyReservations)) 
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Rezervasyon oluşturulurken bir hata oluştu: " + ex.Message });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                         .Select(e => e.ErrorMessage)
                                         .ToList();
            return Json(new { success = false, errors = errors });
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
