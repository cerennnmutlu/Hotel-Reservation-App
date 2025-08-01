namespace HotelReservationApp.Models;

public class HotelAmenity
{
    public int AmenityID { get; set; }
    public string AmenityName { get; set; }
    public string Icon { get; set; }

    public ICollection<HotelAmenityMapping> HotelAmenityMappings { get; set; }
}
