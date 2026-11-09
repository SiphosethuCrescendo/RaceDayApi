using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required"), EmailAddress]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "Password is required"), DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = "";
        [Required(ErrorMessage = "Email is required"), EmailAddress]
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters"), DataType(DataType.Password)]
        public string Password { get; set; } = "";
        [Required] public string Role { get; set; } = "Participant";
    }

    public class AuthUser
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
    }

    public class AuthResponse
    {
        public string Token { get; set; } = "";
        public AuthUser? User { get; set; }
    }
}