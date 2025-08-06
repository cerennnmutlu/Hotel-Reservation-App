using System;

namespace HotelReservationApp.Models
{
    public class ReservationViewModel
    {
        public int RoomID { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int GuestCount { get; set; }
        public string SpecialRequests { get; set; }
        
        // Reservation modeline dönüştürme metodu
        public Reservation ToReservation()
        {
            return new Reservation
            {
                RoomID = this.RoomID,
                CheckInDate = this.CheckInDate,
                CheckOutDate = this.CheckOutDate,
                GuestCount = this.GuestCount,
                SpecialRequests = this.SpecialRequests,
                Status = "Confirmed",
                CreatedDate = DateTime.Now
            };
        }
    }
}