namespace GUI_BRAP.Models
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Http;

    public class ProfileViewModel
    {
        [Display(Name = "Username")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Display Name is required.")]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid Phone Number.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? Country { get; set; }

        public string? City { get; set; }

        [Display(Name = "Street Name")]
        public string? StreetName { get; set; }

        [Display(Name = "Street Number")]
        public string? StreetNumber { get; set; }

        public string? AvatarUrl { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }
}