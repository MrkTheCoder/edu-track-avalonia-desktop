using Avalonia;
using Avalonia.Logging;
using EduTrack.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace EduTrack.App
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices();
            
            // Uncomment for test logging
            TestingLogging(serviceProvider); // pass DI provider

            // Initialize the database before launching Avalonia UI
            InitializeDatabase();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();

        private static void InitializeDatabase()
        {
            try
            {
                var options = new DbContextOptionsBuilder<EduTrackDbContext>()
                    .UseSqlite("Data Source=EduTrack.db")
                    .Options;

                using var context = new EduTrackDbContext(options);
                DatabaseInitializer.Initialize(context);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Database setup error:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole(); // shows logs in terminal
                builder.AddDebug();   // shows logs in VS Debug output
                builder.SetMinimumLevel(LogLevel.Information); // show info, warning, error
            });

            // register DbContext for design-time / runtime, repository & services will be added later
            services.AddDbContext<EduTrackDbContext>(options =>
                options.UseSqlite("Data Source=EduTrack.db"));

            return services.BuildServiceProvider();
        }

        private static void TestingLogging(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            Console.WriteLine("Console.WriteLine test output"); // always visible in terminal
            logger.LogInformation("ILogger test output - Information level");
            logger.LogWarning("ILogger test output - Warning level");
            logger.LogError("ILogger test output - Error level");
        }

    }
}
