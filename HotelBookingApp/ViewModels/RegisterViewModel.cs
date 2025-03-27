using System.ComponentModel.DataAnnotations;

namespace HotelBookingApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Role { get; set; }  // "Admin" or "User"
    }
}
