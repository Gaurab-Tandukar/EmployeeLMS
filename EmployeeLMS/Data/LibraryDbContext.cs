using Microsoft.EntityFrameworkCore;
using EmployeeLMS.Models;

namespace EmployeeLMS.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options) { }

        public DbSet<BookAsset> BookAssets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BookAssetAssignment> BookAssetAssignments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Admin> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------- Employee ----------
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.StaffID);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.HashPassword)
                    .IsRequired()
                    .HasMaxLength(255);

                // Prevent duplicate employee accounts by email
                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });

            // ---------- User ----------
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserID);

                entity.Property(u => u.UserRole)
                    .IsRequired()
                    .HasMaxLength(50);

                // Enforce true one-to-one: no two Users can share the same StaffID
                entity.HasIndex(u => u.StaffID)
                    .IsUnique();

                // One-to-one: User -> Employee (required)
                entity.HasOne(u => u.Employee)
                    .WithOne(e => e.User)
                    .HasForeignKey<User>(u => u.StaffID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Admin ----------
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(a => a.AdminId);

                entity.Property(a => a.Name)
                    .IsRequired()
                    .HasMaxLength(40);

                // Many Admins -> one User
                entity.HasOne(a => a.User)
                    .WithMany(u => u.Admins)
                    .HasForeignKey(a => a.UserID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- BookAsset ----------
            modelBuilder.Entity<BookAsset>(entity =>
            {
                entity.HasKey(b => b.BookId);

                entity.Property(b => b.SerialNo)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(b => b.SerialNo)
                    .IsUnique();

                entity.Property(b => b.Status)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(b => b.Cost)
                    .HasColumnType("decimal(10,2)");
            });

            // ---------- Category ----------
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.CategoryId);

                entity.Property(c => c.CategoryName)
                    .IsRequired()
                    .HasMaxLength(100);

                // Many Categories -> one BookAsset
                entity.HasOne(c => c.BookAsset)
                    .WithMany(b => b.Categories)
                    .HasForeignKey(c => c.BookID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- BookAssetAssignment ----------
            modelBuilder.Entity<BookAssetAssignment>(entity =>
            {
                entity.HasKey(a => a.AssignmentID);

                entity.Property(a => a.AssignedDate)
                    .IsRequired();

                // ReturnDate is intentionally nullable - not set until the asset is returned

                // Many Assignments -> one BookAsset
                entity.HasOne(a => a.BookAsset)
                    .WithMany(b => b.BookAssetAssignments)
                    .HasForeignKey(a => a.BookID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Many Assignments -> one User
                entity.HasOne(a => a.User)
                    .WithMany(u => u.BookAssetAssignments)
                    .HasForeignKey(a => a.UserID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}