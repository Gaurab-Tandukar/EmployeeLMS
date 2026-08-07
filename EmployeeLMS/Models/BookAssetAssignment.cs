namespace EmployeeLMS.Models
{
    public class BookAssetAssignment
    {
        public int AssignmentID { get; set; }

        public int BookID { get; set; }
        public virtual BookAsset BookAsset { get; set; } = null!;

        public int UserID { get; set; }
        public virtual User User { get; set; } = null!;

        public DateTime AssignedDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}