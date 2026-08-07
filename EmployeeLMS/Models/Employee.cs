using System.ComponentModel.DataAnnotations;

namespace EmployeeLMS.Models
{
    public class Employee
    {
        public int StaffID { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

        // Optional side of the 1:1 relationship with User
        public virtual User? User { get; set; }

    }
}