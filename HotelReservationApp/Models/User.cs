namespace HotelReservationApp.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int RoleID { get; set; }
        public string Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;

        public Role Role { get; set; }
        public ICollection<Hotel> Hotels { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
