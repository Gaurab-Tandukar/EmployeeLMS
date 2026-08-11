using System.ComponentModel.DataAnnotations;

namespace EmployeeLMS.Models
{
    public class User
    {
        public int UserID { get; set; }
        public int StaffID { get; set; }
        [Required]
        public string UserRole { get; set; }

        // Required side of the 1:1 relationship with Employee
        public virtual Employee Employee { get; set; } = null!;

        // One-to-many navigation collections
        public virtual ICollection<Admin> Admins { get; set; } = new List<Admin>();
        public virtual ICollection<BookAssetAssignment> BookAssetAssignments { get; set; } = new List<BookAssetAssignment>();
    }
}