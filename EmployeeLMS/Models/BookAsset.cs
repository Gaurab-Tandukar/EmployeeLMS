namespace EmployeeLMS.Models
{
    public class BookAsset
    {
        public int BookId { get; set; }
        public string SerialNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Cost { get; set; }

        public virtual ICollection<BookAssetAssignment> BookAssetAssignments { get; set; } = new List<BookAssetAssignment>();
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}