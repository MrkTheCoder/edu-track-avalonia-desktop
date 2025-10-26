using EduTrack.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Data.Context
{
    // Class definition called: `Primary Constructor`
    public class EduTrackDbContext(DbContextOptions<EduTrackDbContext> options) 
        : DbContext(options)
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=EduTrack.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.LastName);
                entity.Property(e => e.FirstName).HasMaxLength(100).IsUnicode(true);
                entity.Property(e => e.LastName).HasMaxLength(100).IsUnicode(true);
                entity.Property(e => e.Email).HasMaxLength(255).IsUnicode(true);
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).HasMaxLength(255).IsUnicode(true);
                entity.Property(e => e.Description).HasMaxLength(500).IsUnicode(true);
            });
        }

    }
}