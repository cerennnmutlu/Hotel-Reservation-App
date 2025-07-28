namespace HotelReservationApp.Models
{
    public class HotelAmenityMapping
    {
        public int HotelID { get; set; }
        public int AmenityID { get; set; }

        public Hotel Hotel { get; set; }
        public HotelAmenity Amenity { get; set; }
    }
} 