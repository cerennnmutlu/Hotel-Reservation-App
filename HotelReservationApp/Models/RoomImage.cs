using System.ComponentModel.DataAnnotations;

namespace HotelReservationApp.Models
{
    public class RoomImage
    {
        [Key]
        public int ImageID { get; set; }
        public int RoomID { get; set; }
        public string ImageUrl { get; set; }

        public Room Room { get; set; }
    }
}
