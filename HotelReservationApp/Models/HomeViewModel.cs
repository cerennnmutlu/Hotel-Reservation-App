using System.Collections.Generic;

namespace HotelReservationApp.Models
{
    public class HomeViewModel
    {
        public int TotalRooms { get; set; }
        public int TotalHotels { get; set; }
        public int TotalCustomers { get; set; }
        public List<Room> FeaturedRooms { get; set; } = new List<Room>();
        public List<Hotel> FeaturedHotels { get; set; } = new List<Hotel>();
    }
} 