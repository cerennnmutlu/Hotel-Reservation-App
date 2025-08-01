namespace HotelReservationApp.Models
{
    public class City
    {
        public int CityID { get; set; }
        public string CityName { get; set; }

        public ICollection<Hotel> Hotels { get; set; }
    }
}
