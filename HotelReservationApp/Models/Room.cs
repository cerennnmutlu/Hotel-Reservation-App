namespace HotelReservationApp.Models
{
    public class Room
    {
        public int RoomID { get; set; }
        public int HotelID { get; set; }
        public int RoomTypeID { get; set; }
        public string RoomNumber { get; set; }
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; } = true;

        public Hotel Hotel { get; set; }
        public RoomType RoomType { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<RoomImage> RoomImages { get; set; }
    }
}
