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
    }
}