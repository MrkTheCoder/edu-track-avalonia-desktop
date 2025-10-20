using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EduTrack.Data.Context
{
    public class EduTrackDbContextFactory : IDesignTimeDbContextFactory<EduTrackDbContext>
    {
        public EduTrackDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EduTrackDbContext>();
            optionsBuilder.UseSqlite("Data Source=EduTrack.db");

            return new EduTrackDbContext(optionsBuilder.Options);
        }
    }
}