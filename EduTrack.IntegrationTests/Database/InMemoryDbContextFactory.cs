using EduTrack.Data.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EduTrack.IntegrationTests.Database
{
    // This helps when we want multiple contexts within one test (e.g., simulating concurrent requests).
    public static class InMemoryDbContextFactory
    {

        /*
            In some test suites, If We’ll want:
              - Need multiple contexts sharing the same connection to simulate realistic transactions, Call `CreateNewContext(sharedConnection)`. 
              - Need full isolation, Call `CreateNewContext()`.
        */
        public static EduTrackDbContext CreateNewContext(SqliteConnection? sharedConnection = null)
        {
            var connection = sharedConnection ?? new SqliteConnection("Filename=:memory:");
            if (connection.State != ConnectionState.Open)
                connection.Open();

            var options = new DbContextOptionsBuilder<EduTrackDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new EduTrackDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
