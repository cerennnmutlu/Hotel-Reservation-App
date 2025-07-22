public class User
{
    public int UserID { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public int RoleID { get; set; }

    public Role Role { get; set; }
    public ICollection<Hotel> Hotels { get; set; }
    public ICollection<Reservation> Reservations { get; set; }
    public ICollection<Review> Reviews { get; set; }
} 