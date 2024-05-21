using System.ComponentModel.DataAnnotations;

namespace Bloggie.Web.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        [Required]
        public string oldPassword { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password has to be at least 6 characters")]
        public string newPassword { get; set; }

        [Required]
        public string confirmNewPassword { get; set; }
        

    }
}
