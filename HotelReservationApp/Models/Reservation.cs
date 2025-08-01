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
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public int GuestCount { get; set; }
        public string SpecialRequests { get; set; }
        public DateTime? CancellationDate { get; set; }

        public User User { get; set; }
        public Room Room { get; set; }
        public Payment Payment { get; set; }
    }
}
