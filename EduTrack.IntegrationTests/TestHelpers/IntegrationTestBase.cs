using EduTrack.Data.Context;
using EduTrack.IntegrationTests.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EduTrack.IntegrationTests.TestHelpers
{
    public class IntegrationTestBase : IDisposable, IAsyncDisposable
    {
        protected readonly SqliteConnection Connection;
        protected readonly DbContextOptions<EduTrackDbContext> Options;
        protected readonly EduTrackDbContext Context;
        protected readonly ILoggerFactory LoggerFactory;

        public IntegrationTestBase()
        {
            // ✅ Setup logger (writes to debug output)
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            var logger = LoggerFactory.CreateLogger<IntegrationTestBase>();
            logger.LogWarning("✅ Logger initialized successfully.");
            Console.WriteLine("Console output test successful!");

            // Create a single in-memory connection for this test
            Connection = new SqliteConnection("Filename=:memory:");
            Connection.Open();

            Options = new DbContextOptionsBuilder<EduTrackDbContext>()
                .UseSqlite(Connection)
                .UseLoggerFactory(LoggerFactory) // wire EF logs to our factory
                .EnableSensitiveDataLogging() // helpful in tests
                .Options;

            Context = new EduTrackDbContext(Options);

            // Use `EnsureCreated()`    for early or fast-running integration tests.
            // Use `Migrate()`          for validating database constraints or migration logic.
            const bool useMigrations = false;
            try
            {
                if (useMigrations)
                    Context.Database.Migrate();
                else
                    Context.Database.EnsureCreated();
                /*
                    `Migrate()` runs all migrations, ensuring everything is applied — useful once our schema grows and constraints matter.
                        → Slightly slower, but more realistic.
                     
                    `EnsureCreated()`builds the schema directly from the EF model, skipping migrations.
                        → It’s faster, but ignores constraints, annotations, `OnModelCreating`, and raw SQL defined in migrations (e.g., indexes, triggers, default values).
                        → Good for early-stage or fast tests.
                 */
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database initialization failed: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            Context?.Dispose();
            Connection?.Dispose();
            LoggerFactory?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await Connection.DisposeAsync();
        }

        protected async Task ResetDatabaseAsync()
        {
            await Context.Database.EnsureDeletedAsync();
            await Context.Database.EnsureCreatedAsync();
            await SeedTestDataAsync();
        }

        protected virtual Task SeedTestDataAsync()
        {
            TestDataSeeder.SeedBasicData(Context);
            return Context.SaveChangesAsync();
        }
    }
}