using System;
using System.Linq;
using System.Threading.Tasks;
using HotelReservationApp.Data;
using HotelReservationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationApp.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly HotelReservationContext _context;

        public ReviewController(HotelReservationContext context)
        {
            _context = context;
        }

        // Review oluşturma sayfası (GET)
        public async Task<IActionResult> Create(int hotelId)
        {
            var hotel = await _context.Hotels.FindAsync(hotelId);
            if (hotel == null) return NotFound();

            ViewBag.HotelName = hotel.Name;
            ViewBag.HotelId = hotelId;
            return View(); // Views/Review/Create.cshtml
        }

        // Review gönderme işlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review review)
        {
            if (ModelState.IsValid)
            {
                review.UserID = int.Parse(User.Identity.Name); // Auth kullanıcı ID'si
                review.ReviewDate = DateTime.Now;
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction("HotelReviews", new { hotelId = review.HotelID });
            }

            ViewBag.HotelId = review.HotelID;
            return View(review);
        }

        // Belirli bir otelin tüm yorumları
        [AllowAnonymous]
        public async Task<IActionResult> HotelReviews(int hotelId)
        {
            var hotel = await _context.Hotels.FindAsync(hotelId);
            if (hotel == null) return NotFound();

            var reviews = await _context.Reviews
                .Where(r => r.HotelID == hotelId)
                .Include(r => r.User)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            ViewBag.HotelName = hotel.Name;
            return View(reviews); // Views/Review/HotelReviews.cshtml
        }
    }
}
