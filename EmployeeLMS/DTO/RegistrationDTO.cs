using System.ComponentModel.DataAnnotations;

namespace EmployeeLMS.DTO
{
    public class RegistrationDTO
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; }

        [Required, Phone, MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required, MinLength(8)]
        public string Password { get; set; }

        [Required, Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
        
    }
}
