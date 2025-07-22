namespace HotelReservationApp.Models
{
    public class RoomType
    {
        public int RoomTypeID { get; set; }
        public string TypeName { get; set; }

        public ICollection<Room> Rooms { get; set; }
    }
} 