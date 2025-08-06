using System.ComponentModel.DataAnnotations;

namespace HotelReservationApp.Models
{
    public class ReservationDetailsViewModel
    {
        public Reservation Reservation { get; set; }
        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
        public bool CanAddReview { get; set; }
        public bool HasUserReviewed { get; set; }
        public string CurrentUserId { get; set; }
    }

    public class ReviewViewModel
    {
        public int ReviewID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int HotelID { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime? ReviewDate { get; set; }
        public int? ReservationID { get; set; }
    }

    public class AddReviewViewModel
    {
        public int ReservationID { get; set; }
        public int HotelID { get; set; }
        
        [Required(ErrorMessage = "Puan zorunludur")]
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
        public int Rating { get; set; }
        
        [Required(ErrorMessage = "Yorum zorunludur")]
        [StringLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir")]
        public string Comment { get; set; }
    }
}