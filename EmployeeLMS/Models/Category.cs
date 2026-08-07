namespace EmployeeLMS.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int BookID { get; set; }
        public virtual BookAsset BookAsset { get; set; } = null!;
    }
}