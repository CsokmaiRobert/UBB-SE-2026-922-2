using System.ComponentModel.DataAnnotations;

namespace GUI_BRAP.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Display name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "City")]
        public string? City { get; set; }

        [Display(Name = "Street name")]
        public string? StreetName { get; set; }

        [Display(Name = "Street number")]
        public string? StreetNumber { get; set; }
    }
}
