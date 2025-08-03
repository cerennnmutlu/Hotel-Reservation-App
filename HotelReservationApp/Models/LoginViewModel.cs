using System.ComponentModel.DataAnnotations;

namespace HotelReservationApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email boş bırakılamaz")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Şifre boş bırakılamaz")]
        public string Password { get; set; }
    }
}
