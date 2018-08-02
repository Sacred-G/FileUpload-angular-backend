using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Credentials
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
