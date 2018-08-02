using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class User
    {
        [Required(ErrorMessage = "Name is required")]
        [MinLength(5, ErrorMessage = "Name of the user must be at least 5 characters long.")]
        public string Name { get; set; }
    }
}