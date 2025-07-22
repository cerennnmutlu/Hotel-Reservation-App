using System.ComponentModel.DataAnnotations;

namespace HotelReservationApp.Models
{
    public class HotelImage
    {
        [Key]
        public int ImageID { get; set; }
        public int HotelID { get; set; }
        public string ImageUrl { get; set; }

        public Hotel Hotel { get; set; }
    }
} 