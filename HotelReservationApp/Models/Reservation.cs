namespace HotelReservationApp.Models
{
    public class Reservation
    {
        public int ReservationID { get; set; }
        public int UserID { get; set; }
        public int RoomID { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; }

        public User User { get; set; }
        public Room Room { get; set; }
        public Payment Payment { get; set; }
    }
} 