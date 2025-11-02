using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EduTrack.Data.Context
{
    /*
          Because our startup project (EduTrack.App) is a UI app and doesn’t use dependency injection yet,
        EF can’t figure out how to build your EduTrackDbContext instance — hence the error:
        
            Unable to create a 'DbContext' of type 'RuntimeType'. The exception 'Unable to resolve service
            for type 'Microsoft.EntityFrameworkCore.DbContextOptions`1[EduTrack.Data.Context.EduTrackDbContext]'
            while attempting to activate 'EduTrack.Data.Context.EduTrackDbContext'.' was thrown while attempting
            to create an instance. For the different patterns supported at design time, 
            see https://go.microsoft.com/fwlink/?linkid=851728
        
          EF Core tools try to instantiate our DbContext at design time (during Add-Migration) using one of
        several discovery methods (DI, factory, etc.). This is the standard fix for desktop projects, and 
        it’s exactly what EF’s documentation recommends.

          What This Does:
        - It gives EF Core Tools an explicit way to create our DbContext during migrations.
        - It’s only used at design time — it won’t affect runtime behavior.
        - Works perfectly for Avalonia, WPF, WinUI, and MAUI apps.
     */
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