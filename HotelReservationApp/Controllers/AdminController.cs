using HotelReservationApp.Data;
using HotelReservationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public async Task<IActionResult> Index()
        {
            ViewBag.UserCount = await _context.Users.CountAsync();
            ViewBag.HotelCount = await _context.Hotels.CountAsync();
            ViewBag.ReservationCount = await _context.Reservations.CountAsync();
            ViewBag.ReviewCount = await _context.Reviews.CountAsync();
            return View();
        }

        // USERS
        public async Task<IActionResult> Users() => View(await _context.Users.Include(u => u.Role).ToListAsync());

        public IActionResult AddUserForm() => PartialView("_UserForm", new User());

        public async Task<IActionResult> EditUserForm(int id) =>
            PartialView("_UserForm", await _context.Users.FindAsync(id));

        [HttpPost]
        public async Task<IActionResult> AddUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        // HOTELS
        public async Task<IActionResult> Hotels() => View(await _context.Hotels.Include(h => h.City).ToListAsync());
        
        // HOTEL ROOMS
        public async Task<IActionResult> HotelRooms(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }
            
            ViewBag.HotelName = hotel.Name;
            ViewBag.HotelID = hotel.HotelID;
            
            var rooms = await _context.Rooms
                .Where(r => r.HotelID == id)
                .Include(r => r.RoomType)
                .ToListAsync();
                
            return View(rooms);
        }
        
        [Route("Admin/AddRoomForm/{hotelId}")]
        public async Task<IActionResult> AddRoomForm(int hotelId)
        {
            var hotel = await _context.Hotels.FindAsync(hotelId);
            if (hotel == null)
            {
                return NotFound();
            }
            
            ViewBag.HotelName = hotel.Name;
            ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();
            
            var room = new Room { HotelID = hotelId };
            return PartialView("_RoomForm", room);
        }
        
        [Route("Admin/EditRoomForm/{id}")]
        public async Task<IActionResult> EditRoomForm(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            
            var hotel = await _context.Hotels.FindAsync(room.HotelID);
            ViewBag.HotelName = hotel?.Name;
            ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();
            
            return PartialView("_RoomForm", room);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddRoom([FromForm] Room room, List<IFormFile> roomImages)
        {
            try
            {
                Console.WriteLine($"Received room data: HotelID={room.HotelID}, RoomTypeID={room.RoomTypeID}, RoomNumber={room.RoomNumber}, PricePerNight={room.PricePerNight}, Capacity={room.Capacity}");
                
                // Validate required fields
                var validationErrors = new List<string>();
                
                if (room.HotelID <= 0)
                {
                    validationErrors.Add("Invalid Hotel ID");
                }
                
                if (room.RoomTypeID <= 0)
                {
                    validationErrors.Add("Please select a valid room type");
                }
                
                if (string.IsNullOrWhiteSpace(room.RoomNumber))
                {
                    validationErrors.Add("Room number is required");
                }
                
                if (room.PricePerNight <= 0)
                {
                    validationErrors.Add("Price per night must be greater than zero");
                }
                
                if (room.Capacity <= 0)
                {
                    validationErrors.Add("Capacity must be greater than zero");
                }
                
                if (validationErrors.Any())
                {
                    Console.WriteLine($"Validation failed: {string.Join(", ", validationErrors)}");
                    return BadRequest(new { success = false, message = string.Join(", ", validationErrors) });
                }
                
                // Check if hotel exists
                var hotel = await _context.Hotels.FindAsync(room.HotelID);
                if (hotel == null)
                {
                    Console.WriteLine($"Hotel with ID {room.HotelID} not found");
                    return NotFound(new { success = false, message = $"Hotel with ID {room.HotelID} not found" });
                }
                
                // Check if room type exists
                var roomType = await _context.RoomTypes.FindAsync(room.RoomTypeID);
                if (roomType == null)
                {
                    Console.WriteLine($"Room type with ID {room.RoomTypeID} not found");
                    return NotFound(new { success = false, message = $"Room type with ID {room.RoomTypeID} not found" });
                }
                
                // Check if room number already exists for this hotel
                var existingRoom = await _context.Rooms
                    .Where(r => r.HotelID == room.HotelID && r.RoomNumber == room.RoomNumber)
                    .FirstOrDefaultAsync();
                    
                if (existingRoom != null)
                {
                    Console.WriteLine($"Room with number {room.RoomNumber} already exists in hotel {room.HotelID}");
                    return StatusCode(409, new { success = false, error = $"Room with number {room.RoomNumber} already exists in this hotel" });
                }
                
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Room added successfully with ID: {room.RoomID}");
                
                // Handle image uploads
                if (roomImages != null && roomImages.Count > 0)
                {
                    Console.WriteLine($"Processing {roomImages.Count} room images");
                    
                    // Create directory if it doesn't exist
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "rooms", room.RoomID.ToString());
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    foreach (var image in roomImages)
                    {
                        if (image.Length > 0)
                        {
                            // Generate a unique filename
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                            string filePath = Path.Combine(uploadsFolder, fileName);
                            
                            // Save the file
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }
                            
                            // Save image info to database
                            var roomImage = new RoomImage
                            {
                                RoomID = room.RoomID,
                                ImageUrl = $"/img/rooms/{room.RoomID}/{fileName}"
                            };
                            
                            _context.RoomImages.Add(roomImage);
                            Console.WriteLine($"Added image: {roomImage.ImageUrl}");
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Room images saved successfully");
                }
                
                return Ok(new { success = true, redirectUrl = Url.Action("HotelRooms", new { id = room.HotelID }) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddRoom: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new { success = false, error = "An error occurred while adding the room. Please try again." });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> EditRoom([FromForm] Room room, List<IFormFile> roomImages)
        {
            try
            {
                Console.WriteLine($"Received edit room data: RoomID={room.RoomID}, HotelID={room.HotelID}, RoomTypeID={room.RoomTypeID}, RoomNumber={room.RoomNumber}, PricePerNight={room.PricePerNight}, Capacity={room.Capacity}");
                
                // Validate required fields
                var validationErrors = new List<string>();
                
                if (room.RoomID <= 0)
                {
                    validationErrors.Add("Invalid Room ID");
                }
                
                if (room.HotelID <= 0)
                {
                    validationErrors.Add("Invalid Hotel ID");
                }
                
                if (room.RoomTypeID <= 0)
                {
                    validationErrors.Add("Please select a valid room type");
                }
                
                if (string.IsNullOrWhiteSpace(room.RoomNumber))
                {
                    validationErrors.Add("Room number is required");
                }
                
                if (room.PricePerNight <= 0)
                {
                    validationErrors.Add("Price per night must be greater than zero");
                }
                
                if (room.Capacity <= 0)
                {
                    validationErrors.Add("Capacity must be greater than zero");
                }
                
                if (validationErrors.Any())
                {
                    Console.WriteLine($"Validation failed: {string.Join(", ", validationErrors)}");
                    return BadRequest(new { success = false, message = string.Join(", ", validationErrors) });
                }
                
                // Check if room exists
                var existingRoom = await _context.Rooms.FindAsync(room.RoomID);
                if (existingRoom == null)
                {
                    Console.WriteLine($"Room with ID {room.RoomID} not found");
                    return NotFound(new { success = false, message = $"Room with ID {room.RoomID} not found" });
                }
                
                // Check if room number already exists for this hotel (excluding the current room)
                var duplicateRoom = await _context.Rooms
                    .Where(r => r.HotelID == room.HotelID && r.RoomNumber == room.RoomNumber && r.RoomID != room.RoomID)
                    .FirstOrDefaultAsync();
                    
                if (duplicateRoom != null)
                {
                    Console.WriteLine($"Another room with number {room.RoomNumber} already exists in hotel {room.HotelID}");
                    return StatusCode(409, new { success = false, error = $"Another room with number {room.RoomNumber} already exists in this hotel" });
                }
                
                // Update room properties
                existingRoom.RoomTypeID = room.RoomTypeID;
                existingRoom.RoomNumber = room.RoomNumber;
                existingRoom.PricePerNight = room.PricePerNight;
                existingRoom.Capacity = room.Capacity;
                existingRoom.IsAvailable = room.IsAvailable;
                
                await _context.SaveChangesAsync();
                Console.WriteLine($"Room updated successfully: {room.RoomID}");
                
                // Handle image uploads
                if (roomImages != null && roomImages.Count > 0)
                {
                    Console.WriteLine($"Processing {roomImages.Count} room images for edit");
                    
                    // Create directory if it doesn't exist
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "rooms", room.RoomID.ToString());
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    foreach (var image in roomImages)
                    {
                        if (image.Length > 0)
                        {
                            // Generate a unique filename
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                            string filePath = Path.Combine(uploadsFolder, fileName);
                            
                            // Save the file
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }
                            
                            // Save image info to database
                            var roomImage = new RoomImage
                            {
                                RoomID = room.RoomID,
                                ImageUrl = $"/img/rooms/{room.RoomID}/{fileName}"
                            };
                            
                            _context.RoomImages.Add(roomImage);
                            Console.WriteLine($"Added image: {roomImage.ImageUrl}");
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Room images saved successfully");
                }
                
                return Ok(new { success = true, redirectUrl = Url.Action("HotelRooms", new { id = room.HotelID }) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EditRoom: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new { success = false, error = "An error occurred while updating the room. Please try again." });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            
            int hotelId = room.HotelID;
            
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            
            return Ok(new { redirectUrl = Url.Action("HotelRooms", new { id = hotelId }) });
        }
        
        [Route("Admin/RoomDetails/{id}")]
        public async Task<IActionResult> RoomDetails(int id)
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

            return PartialView("_RoomDetailsModal", room);
        }

        public async Task<IActionResult> AddHotelForm()
        {
            ViewBag.Cities = await _context.Cities.ToListAsync();
            // Get hotel managers (users with RoleID = 2)
            ViewBag.HotelManagers = await _context.Users
                .Where(u => u.RoleID == 2)
                .ToListAsync();
            return PartialView("_HotelForm", new Hotel());
        }

        public async Task<IActionResult> EditHotelForm(int id)
        {
            ViewBag.Cities = await _context.Cities.ToListAsync();
            // Get hotel managers (users with RoleID = 2)
            ViewBag.HotelManagers = await _context.Users
                .Where(u => u.RoleID == 2)
                .ToListAsync();
            return PartialView("_HotelForm", await _context.Hotels.FindAsync(id));
        }

        public async Task<IActionResult> ViewHotelDetails(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.City)
                .FirstOrDefaultAsync(h => h.HotelID == id);
                
            if (hotel == null)
            {
                return NotFound();
            }
            
            return PartialView("_HotelDetails", hotel);
        }

        [HttpPost]
        public async Task<IActionResult> AddHotel(Hotel hotel)
        {
            if (hotel.CreatedDate == default(DateTime))
            {
                hotel.CreatedDate = DateTime.Now;
            }
            
            if (hotel.IsActive == false && hotel.HotelID == 0)
            {
                hotel.IsActive = true; // Default to active for new hotels
            }
            
            // Owner is now selected from the form
            if (hotel.OwnerID == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir otel sahibi seçin";
                return RedirectToAction("Hotels");
            }
            
            try
            {
                _context.Hotels.Add(hotel);
                await _context.SaveChangesAsync();
                return RedirectToAction("Hotels");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine(ex.Message);
                
                // Redirect with error message
                TempData["ErrorMessage"] = "Hotel eklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Hotels");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditHotel(Hotel hotel)
        {
            var existingHotel = await _context.Hotels.AsNoTracking().FirstOrDefaultAsync(h => h.HotelID == hotel.HotelID);
            if (existingHotel != null)
            {
                if (hotel.CreatedDate == default(DateTime))
                {
                    hotel.CreatedDate = existingHotel.CreatedDate;
                }
                
                // Preserve the OwnerID from the existing hotel
                hotel.OwnerID = existingHotel.OwnerID;
            }
            
            try
            {
                _context.Hotels.Update(hotel);
                await _context.SaveChangesAsync();
                return RedirectToAction("Hotels");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine(ex.Message);
                
                // Redirect with error message
                TempData["ErrorMessage"] = "Hotel güncellenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Hotels");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel != null)
            {
                _context.Hotels.Remove(hotel);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        // RESERVATIONS
        public async Task<IActionResult> Reservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Room)
                    .ThenInclude(room => room.Hotel)
                .ToListAsync();

            return View(reservations);
        }
        
        [HttpPost]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                reservation.Status = "Cancelled";
                reservation.CancellationDate = DateTime.Now;
                reservation.UpdatedDate = DateTime.Now;
                
                // Make the room available again
                var room = await _context.Rooms.FindAsync(reservation.RoomID);
                if (room != null)
                {
                    room.IsAvailable = true;
                    _context.Rooms.Update(room);
                }
                
                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }


        // REVIEWS
        public async Task<IActionResult> Reviews() =>
            View(await _context.Reviews.Include(r => r.User).Include(r => r.Hotel).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
        
        // API endpoint to get room types
        [HttpGet]
        public async Task<IActionResult> GetRoomTypes()
        {
            var roomTypes = await _context.RoomTypes.ToListAsync();
            var formattedRoomTypes = roomTypes.Select(rt => new
            {
                roomTypeID = rt.RoomTypeID,
                name = rt.TypeName
            });
            return Json(formattedRoomTypes);
        }
        
        // API endpoint to get cities
        [HttpGet]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _context.Cities.ToListAsync();
            return Json(cities);
        }
    }
}