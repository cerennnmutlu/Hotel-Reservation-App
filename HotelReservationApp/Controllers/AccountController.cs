using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservationApp.Data;
using HotelReservationApp.Models;

namespace HotelReservationApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly HotelReservationContext _context;

        public AccountController(HotelReservationContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Debug için log
            Console.WriteLine($"Login attempt - Username: {username}, Password: {password}");
            
            // Veritabanından kullanıcıyı bul
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == username);

            Console.WriteLine($"User found: {user != null}");
            if (user != null)
            {
                Console.WriteLine($"User ID: {user.UserID}, Name: {user.FullName}, Role: {user.Role?.RoleName}");
                Console.WriteLine($"Password match: {user.PasswordHash == password}");
            }

            if (user != null && user.PasswordHash == password) // Gerçek uygulamada hash kontrolü yapılmalı
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.RoleName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Kullanıcı rolüne göre yönlendirme
                switch (user.Role.RoleName.ToLower())
                {
                    case "admin":
                        return RedirectToAction("Index", "Admin");
                    case "hotel manager":
                        return RedirectToAction("Index", "HotelManager"); // DÜZELTİLDİ
                    case "customer":
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Geçersiz kullanıcı adı veya şifre.";
            return View();
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/TestUsers - Geçici test action'ı
        public async Task<IActionResult> TestUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
            
            return View(users);
        }
    }
}