using Avalonia;
using EduTrack.Data.Context;
using Microsoft.EntityFrameworkCore;
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
    }
}
