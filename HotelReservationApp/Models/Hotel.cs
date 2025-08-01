namespace HotelReservationApp.Models
{
    public class Hotel
    {
        public int HotelID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CityID { get; set; }
        public int OwnerID { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }

        public City City { get; set; }
        public User Owner { get; set; }
        public ICollection<Room> Rooms { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<HotelImage> HotelImages { get; set; }
        public ICollection<HotelAmenityMapping> HotelAmenityMappings { get; set; }
    }
}
