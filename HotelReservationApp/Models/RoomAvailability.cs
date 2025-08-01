namespace HotelReservationApp.Models
{
    public class RoomAvailability
    {
        public int AvailabilityID { get; set; }
        public int RoomID { get; set; }
        public DateTime Date { get; set; }
        public bool IsAvailable { get; set; }
        public decimal Price { get; set; }

        public Room Room { get; set; }
    }
}
