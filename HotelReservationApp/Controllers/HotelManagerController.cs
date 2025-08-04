using HotelReservationApp.Data;
using HotelReservationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using Microsoft.AspNetCore.Hosting;


namespace HotelReservationApp.Controllers
{
    [Authorize(Roles = "Hotel Manager")]
    public class HotelManagerController : Controller
    {
        private readonly HotelReservationContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HotelManagerController(HotelReservationContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        
        // View all rooms across all hotels owned by the manager
        public async Task<IActionResult> MyRooms()
        {
            var userId = GetCurrentUserId();
            var rooms = await _context.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .Include(r => r.RoomImages)
                .Where(r => r.Hotel.OwnerID == userId)
                .ToListAsync();
                
            return View(rooms);
        }
        
        // View all reservations for all hotels owned by the manager
        public async Task<IActionResult> MyReservations()
        {
            var userId = GetCurrentUserId();
            var reservations = await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(r => r.Hotel)
                .Include(r => r.User)
                .Include(r => r.Room.RoomType)
                .Include(r => r.Room.RoomImages)
                .Where(r => r.Room.Hotel.OwnerID == userId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
                
            return View(reservations);
        }
        
        // View all reviews for all hotels owned by the manager
        public async Task<IActionResult> MyReviews()
        {
            var userId = GetCurrentUserId();
            var reviews = await _context.Reviews
                .Include(r => r.Hotel)
                    .ThenInclude(h => h.City)
                .Include(r => r.User)
                .Where(r => r.Hotel.OwnerID == userId)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
                
            return View(reviews);
        }
        
        // Update reservation status (confirm or cancel)
        [HttpPost]
        public async Task<IActionResult> UpdateReservationStatus(int id, string status)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                    .ThenInclude(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.ReservationID == id);
                
            if (reservation == null)
                return NotFound();
                
            // Check if the hotel belongs to the current user
            if (!await IsHotelOwner(reservation.Room.HotelID))
                return Forbid();
                
            // Update status
            reservation.Status = status;
            reservation.UpdatedDate = DateTime.Now;
            
            // If cancelled, set cancellation date
            if (status == "Cancelled")
                reservation.CancellationDate = DateTime.Now;
                
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(MyReservations));
        }
        
        // Get review details for modal view
        [HttpGet]
        public async Task<IActionResult> GetReviewDetails(int id)
        {
            if (!await IsReviewForOwnedHotel(id))
                return Content("<div class='alert alert-danger'>Bu yorumu görüntüleme yetkiniz yok.</div>");
                
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.ReviewID == id);
                
            if (review == null)
                return Content("<div class='alert alert-danger'>Yorum bulunamadı.</div>");
                
            var html = $@"<div class='card'>                
                <div class='card-body'>
                    <h5 class='card-title'>{review.Hotel?.Name} - {review.Rating}/5</h5>
                    <div class='mb-3'>
                        <div class='d-flex align-items-center mb-2'>
                            <div class='me-2'>
                                <i class='fas fa-user-circle fa-2x text-secondary'></i>
                            </div>
                            <div>
                                <strong>{review.User?.FullName}</strong><br>
                                <small class='text-muted'>{review.ReviewDate?.ToString("MMM dd, yyyy")}</small>
                            </div>
                        </div>
                        <div class='mb-2'>
                            <div class='d-flex'>
                                {string.Join("", Enumerable.Range(1, 5).Select(i => $"<i class='fa{(i <= review.Rating ? "s" : "r")} fa-star {(i <= review.Rating ? "text-warning" : "text-muted")}' style='font-size: 1.2rem;'></i>"))}
                            </div>
                        </div>
                    </div>
                    <div class='card mb-3'>
                        <div class='card-body'>
                            <h6 class='card-subtitle mb-2 text-muted'>Yorum</h6>
                            <p class='card-text'>{(string.IsNullOrEmpty(review.Comment) ? "<em>Yorum yapılmamış</em>" : review.Comment)}</p>
                        </div>
                    </div>
                </div>
            </div>";
            
            return Content(html, "text/html");
        }
        
        // Delete a review
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            if (!await IsReviewForOwnedHotel(id))
                return Json(new { success = false, message = "Bu yorumu silme yetkiniz yok." });
                
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return Json(new { success = false, message = "Yorum bulunamadı." });
                
            try
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Yorum başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Yorum silinirken bir hata oluştu: " + ex.Message });
            }
        }
        
        // Helper method to check if a review belongs to a hotel owned by the current user
        private async Task<bool> IsReviewForOwnedHotel(int reviewId)
        {
            var userId = GetCurrentUserId();
            return await _context.Reviews
                .Include(r => r.Hotel)
                .AnyAsync(r => r.ReviewID == reviewId && r.Hotel.OwnerID == userId);
        }
        
        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }
        
        // Helper method to check if hotel belongs to current user
        private async Task<bool> IsHotelOwner(int hotelId)
        {
            var userId = GetCurrentUserId();
            return await _context.Hotels.AnyAsync(h => h.HotelID == hotelId && h.OwnerID == userId);
        }
        
        // Helper method to check if a room belongs to the current user
        private async Task<bool> IsRoomOwner(int roomId)
        {
            var userId = GetCurrentUserId();
            return await _context.Rooms
                .Include(r => r.Hotel)
                .AnyAsync(r => r.RoomID == roomId && r.Hotel.OwnerID == userId);
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

        public async Task<IActionResult> MyHotels()
        {
            var userId = GetCurrentUserId();
            var hotels = await _context.Hotels
                .Include(h => h.City)
                .Include(h => h.Rooms)
                .Where(h => h.OwnerID == userId)
                .ToListAsync();
            return View(hotels);
        }
        
        // Get hotel details by ID
        [HttpGet("HotelManager/GetHotelDetails/{id}")]
        public async Task<IActionResult> GetHotelDetails(int id)
        {
            if (!await IsHotelOwner(id))
                return Json(new { success = false, message = "Bu oteli görüntüleme yetkiniz yok" });
                
            var hotel = await _context.Hotels
                .Include(h => h.City)
                .FirstOrDefaultAsync(h => h.HotelID == id);
                
            if (hotel == null)
                return Json(new { success = false, message = "Otel bulunamadı" });
                
            // Return a simplified object to avoid circular reference issues
            return Json(new {
                hotelID = hotel.HotelID,
                name = hotel.Name,
                description = hotel.Description,
                cityID = hotel.CityID,
                cityName = hotel.City?.CityName,
                address = hotel.Address,
                phone = hotel.Phone,
                email = hotel.Email,
                website = hotel.Website,
                isActive = hotel.IsActive
            });
        }
        
        // Hotel rooms page
        [HttpGet("HotelManager/HotelRooms/{id}")]
        public async Task<IActionResult> HotelRooms(int id)
        {
            if (!await IsHotelOwner(id))
                return RedirectToAction("MyHotels");
                
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.RoomType)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.RoomImages)
                .FirstOrDefaultAsync(h => h.HotelID == id);
                
            if (hotel == null)
                return NotFound();
                
            ViewBag.HotelName = hotel.Name;
            ViewBag.HotelID = hotel.HotelID;
            
            return View(hotel.Rooms.ToList());
        }
        
        [HttpGet]
        public async Task<IActionResult> GetRoomDetails(int id)
        {
            if (!await IsRoomOwner(id))
            {
                return Json(new { success = false, message = "Bu odaya erişim izniniz yok." });
            }

            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Hotel)
                .Include(r => r.RoomImages)
                .FirstOrDefaultAsync(r => r.RoomID == id);

            if (room == null)
            {
                return Json(new { success = false, message = "Oda bulunamadı." });
            }

            return Json(new
            {
                roomID = room.RoomID,
                hotelID = room.HotelID,
                roomNumber = room.RoomNumber,
                roomTypeID = room.RoomTypeID,
                roomTypeName = room.RoomType?.TypeName,
                capacity = room.Capacity,
                price = room.PricePerNight,
                isAvailable = room.IsAvailable,
                roomImages = room.RoomImages?.Select(ri => new
                {
                    imageID = ri.ImageID,
                    imageUrl = ri.ImageUrl
                }).ToList()
            });
        }
        
        [HttpGet]
        public async Task<IActionResult> GetRoomDetailsView(int id)
        {
            if (!await IsRoomOwner(id))
            {
                return Content("<div class='alert alert-danger'>Bu odaya erişim izniniz yok.</div>");
            }

            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Hotel)
                .Include(r => r.RoomImages)
                .FirstOrDefaultAsync(r => r.RoomID == id);

            if (room == null)
            {
                return Content("<div class='alert alert-danger'>Oda bulunamadı.</div>");
            }

            var html = $@"<div class='card'>
                <div class='card-body'>
                    <h5 class='card-title'>{room.RoomNumber} - {room.RoomType?.TypeName}</h5>
                    <div class='row'>
                        <div class='col-md-6'>
                            <div class='mb-3'>
                                {(room.RoomImages != null && room.RoomImages.Any() ? $@"
                                    <div id='roomImageCarousel' class='carousel slide' data-bs-ride='carousel'>
                                        <div class='carousel-inner'>
                                            {string.Join("", room.RoomImages.Select((img, index) => $@"
                                                <div class='carousel-item {(index == 0 ? "active" : "")}'>                                                
                                                    <img src='{img.ImageUrl}' class='d-block w-100 rounded' style='height: 200px; object-fit: cover;' alt='Oda Resmi'>
                                                </div>
                                            "))}
                                        </div>
                                        {(room.RoomImages.Count() > 1 ? $@"
                                            <button class='carousel-control-prev' type='button' data-bs-target='#roomImageCarousel' data-bs-slide='prev'>
                                                <span class='carousel-control-prev-icon' aria-hidden='true'></span>
                                                <span class='visually-hidden'>Önceki</span>
                                            </button>
                                            <button class='carousel-control-next' type='button' data-bs-target='#roomImageCarousel' data-bs-slide='next'>
                                                <span class='carousel-control-next-icon' aria-hidden='true'></span>
                                                <span class='visually-hidden'>Sonraki</span>
                                            </button>
                                        " : "")}
                                    </div>
                                " : "<p class='text-muted'>Bu oda için resim bulunmuyor.</p>")}
                            </div>
                            <p><strong>Otel:</strong> {room.Hotel?.Name}</p>
                            <p><strong>Kapasite:</strong> {room.Capacity} kişi</p>
                            <p><strong>Fiyat:</strong> {room.PricePerNight:C}</p>
                            <p><strong>Durum:</strong> {(room.IsAvailable ? "<span class='badge bg-success'>Müsait</span>" : "<span class='badge bg-danger'>Dolu</span>")}</p>
                        </div>
                        <div class='col-md-6'>
                            <p><strong>Açıklama:</strong></p>
                           
                        </div>
                    </div>
                    
                    <div class='mt-3'>
                        <h6>Oda Özellikleri</h6>
                        <div class='d-flex flex-wrap gap-2'>";

            // RoomAmenityMappings özelliği şu anda mevcut değil
            html += "<p>Oda özellikleri şu anda görüntülenemiyor.</p>";

            html += $@"</div>
                    </div>
                </div>
            </div>";

            return Content(html, "text/html");
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateRoom()
        {
            if (!Request.HasFormContentType)
            {
                return Json(new { success = false, message = "Invalid request format." });
            }

            // Deserialize room data from form
            Room room;
            try
            {
                var roomDataJson = Request.Form["roomData"];
                room = JsonSerializer.Deserialize<Room>(roomDataJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to parse room data: " + ex.Message });
            }

            if (room == null || !ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid room information." });
            }

            if (!await IsHotelOwner(room.HotelID))
            {
                return Json(new { success = false, message = "You don't have permission to add rooms to this hotel." });
            }

            try
            {
                // Check if room number already exists in this hotel
                var roomExists = await _context.Rooms
                    .AnyAsync(r => r.HotelID == room.HotelID && r.RoomNumber == room.RoomNumber);

                if (roomExists)
                {
                    return Json(new { success = false, message = "This room number is already in use." });
                }

                // Set RoomID to 0 to ensure a new room is created
                room.RoomID = 0;
                
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();

                // Handle image uploads
                var files = Request.Form.Files.GetFiles("roomImages");
                if (files != null && files.Count > 0)
                {
                    // Create uploads directory if it doesn't exist
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "rooms");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            // Generate a unique filename
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            // Save the file
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            // Create a room image record
                            var roomImage = new RoomImage
                            {
                                RoomID = room.RoomID,
                                ImageUrl = "/uploads/rooms/" + fileName
                            };

                            _context.RoomImages.Add(roomImage);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Room added successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while adding the room: " + ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateRoom()
        {
            if (!Request.HasFormContentType)
            {
                return Json(new { success = false, message = "Invalid request format." });
            }

            // Deserialize room data from form
            Room room;
            try
            {
                var roomDataJson = Request.Form["roomData"];
                room = JsonSerializer.Deserialize<Room>(roomDataJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to parse room data: " + ex.Message });
            }

            if (room == null || !ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid room information." });
            }

            if (!await IsRoomOwner(room.RoomID))
            {
                return Json(new { success = false, message = "You don't have permission to edit this room." });
            }

            try
            {
                // Check if room number already exists in this hotel (excluding the current room)
                var roomExists = await _context.Rooms
                    .AnyAsync(r => r.HotelID == room.HotelID && r.RoomNumber == room.RoomNumber && r.RoomID != room.RoomID);

                if (roomExists)
                {
                    return Json(new { success = false, message = "This room number is already in use." });
                }

                var existingRoom = await _context.Rooms.FindAsync(room.RoomID);
                if (existingRoom == null)
                {
                    return Json(new { success = false, message = "Room not found." });
                }

                // Update room properties
                existingRoom.RoomNumber = room.RoomNumber;
                existingRoom.RoomTypeID = room.RoomTypeID;
                existingRoom.Capacity = room.Capacity;
                existingRoom.PricePerNight = room.PricePerNight;
                existingRoom.IsAvailable = room.IsAvailable;
                
                await _context.SaveChangesAsync();

                // Check if we need to delete existing images
                bool deleteExistingImages = Request.Form.ContainsKey("deleteExistingImages") && 
                                          Request.Form["deleteExistingImages"] == "true";

                if (deleteExistingImages)
                {
                    // Get existing images
                    var existingImages = await _context.RoomImages
                        .Where(ri => ri.RoomID == room.RoomID)
                        .ToListAsync();

                    // Delete physical files
                    foreach (var image in existingImages)
                    {
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    // Remove from database
                    _context.RoomImages.RemoveRange(existingImages);
                    await _context.SaveChangesAsync();
                }

                // Handle new image uploads
                var files = Request.Form.Files.GetFiles("roomImages");
                if (files != null && files.Count > 0)
                {
                    // Create uploads directory if it doesn't exist
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "rooms");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            // Generate a unique filename
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            // Save the file
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            // Create a room image record
                            var roomImage = new RoomImage
                            {
                                RoomID = room.RoomID,
                                ImageUrl = "/uploads/rooms/" + fileName
                            };

                            _context.RoomImages.Add(roomImage);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Room updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the room: " + ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            if (!await IsRoomOwner(id))
            {
                return Json(new { success = false, message = "You don't have permission to delete this room." });
            }

            try
            {
                var room = await _context.Rooms
                    .Include(r => r.RoomImages)
                    .FirstOrDefaultAsync(r => r.RoomID == id);
                    
                if (room == null)
                {
                    return Json(new { success = false, message = "Room not found." });
                }

                // Check if room has any reservations
                var hasReservations = await _context.Reservations
                    .AnyAsync(r => r.RoomID == id);

                if (hasReservations)
                {
                    return Json(new { success = false, message = "This room cannot be deleted because it has existing reservations." });
                }

                // Delete room images from file system
                if (room.RoomImages != null && room.RoomImages.Any())
                {
                    foreach (var image in room.RoomImages)
                    {
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Room deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting the room: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateHotel([FromBody] Hotel hotel)
        {
            if (hotel == null)
                return Json(new { success = false, message = "Otel verisi alınamadı" });

            if (string.IsNullOrWhiteSpace(hotel.Name) || hotel.CityID <= 0)
                return Json(new { success = false, message = "Geçersiz otel bilgisi" });

            var userId = GetCurrentUserId();
            hotel.OwnerID = userId;
            hotel.CreatedDate = DateTime.Now;
            hotel.IsActive = true;
            hotel.Address = hotel.Address ?? string.Empty;
            hotel.Phone = hotel.Phone ?? string.Empty;
            hotel.Email = hotel.Email ?? string.Empty;
            hotel.Description = hotel.Description ?? string.Empty;
            hotel.Website = hotel.Website ?? string.Empty;

            try
            {
                _context.Hotels.Add(hotel);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Otel başarıyla eklendi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateHotel([FromBody] Hotel hotel)
        {
            if (hotel == null || hotel.HotelID <= 0)
                return Json(new { success = false, message = "Geçersiz otel verisi" });
                
            if (string.IsNullOrWhiteSpace(hotel.Name) || hotel.CityID <= 0)
                return Json(new { success = false, message = "Geçersiz otel bilgisi" });
                
            if (!await IsHotelOwner(hotel.HotelID))
                return Json(new { success = false, message = "Bu oteli düzenleme yetkiniz yok" });
                
            try
            {
                var existingHotel = await _context.Hotels.FindAsync(hotel.HotelID);
                if (existingHotel == null)
                    return Json(new { success = false, message = "Otel bulunamadı" });
                    
                // Update properties
                existingHotel.Name = hotel.Name;
                existingHotel.CityID = hotel.CityID;
                existingHotel.Address = hotel.Address ?? string.Empty;
                existingHotel.Phone = hotel.Phone ?? string.Empty;
                existingHotel.Email = hotel.Email ?? string.Empty;
                existingHotel.Description = hotel.Description ?? string.Empty;
                existingHotel.Website = hotel.Website ?? string.Empty;
                existingHotel.IsActive = hotel.IsActive;
                
                _context.Hotels.Update(existingHotel);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Otel başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }
        
        [HttpPost("HotelManager/DeleteHotel/{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "Geçersiz otel ID" });
                
            if (!await IsHotelOwner(id))
                return Json(new { success = false, message = "Bu oteli silme yetkiniz yok" });
                
            try
            {
                var hotel = await _context.Hotels
                    .Include(h => h.Rooms)
                    .FirstOrDefaultAsync(h => h.HotelID == id);
                    
                if (hotel == null)
                    return Json(new { success = false, message = "Otel bulunamadı" });
                    
                // Check if hotel has any reservations
                var hasReservations = await _context.Reservations
                    .Include(r => r.Room)
                    .AnyAsync(r => r.Room.HotelID == id);
                    
                if (hasReservations)
                    return Json(new { success = false, message = "Bu otele ait rezervasyonlar olduğu için silinemez" });
                    
                // Remove rooms first
                _context.Rooms.RemoveRange(hotel.Rooms);
                
                // Remove hotel
                _context.Hotels.Remove(hotel);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Otel ve tüm odaları başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }
    }
}
