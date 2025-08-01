using System.ComponentModel.DataAnnotations;

namespace HotelReservationApp.Models
{
    public class BookingViewModel
    {
        [Required(ErrorMessage = "Check-in date is required")]
        [Display(Name = "Check-in Date")]
        public string CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required")]
        [Display(Name = "Check-out Date")]
        public string CheckOutDate { get; set; }

        [Required(ErrorMessage = "Adult count is required")]
        [Range(1, 10, ErrorMessage = "Adult count must be between 1-10")]
        [Display(Name = "Adult Count")]
        public int AdultCount { get; set; } = 1;

        [Display(Name = "Child Count")]
        [Range(0, 10, ErrorMessage = "Child count must be between 0-10")]
        public int ChildCount { get; set; } = 0;

        [Display(Name = "City")]
        public int? CityID { get; set; }

        [Display(Name = "Hotel")]
        public int? HotelID { get; set; }

        [Display(Name = "Room Type")]
        public int? RoomTypeID { get; set; }
    }
} 