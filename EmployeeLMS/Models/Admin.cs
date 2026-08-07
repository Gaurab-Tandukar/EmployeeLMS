namespace EmployeeLMS.Models
{
    public class Admin
    {
        public int AdminId { get; set; }
        public int UserID { get; set; }
        public virtual User User { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
    }
}