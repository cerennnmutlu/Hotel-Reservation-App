public class RoomImage
{
    public int ImageID { get; set; }
    public int RoomID { get; set; }
    public string ImageUrl { get; set; }

    public Room Room { get; set; }
} 