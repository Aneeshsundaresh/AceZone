using System.ComponentModel.DataAnnotations;

namespace GameZone.Models
{
    public class ProfileModel
    {

        [Required]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Current Password is required")]
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }


    }
}
