namespace EduTrack.Data.Context
{
    public static class DatabaseInitializer
    {
        public static void Initialize(EduTrackDbContext context)
        {
            try
            {
                // Check if the database already exists
                var databaseExisted = context.Database.CanConnect();


                // ⚙️ Currently uses EnsureCreated for lightweight setup
                // 🔄 In production or after multiple migrations, replace with:
                //     context.Database.Migrate();
                // to apply any pending migrations automatically at runtime.
                // Create the database if it does not exist
                context.Database.EnsureCreated();

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine(databaseExisted
                    ? "🗄️ Database already exists and is accessible."
                    : "✅ New database created successfully.");

                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Database initialization failed!");
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();

                // Optional: rethrow or handle gracefully depending on the app needs
                throw;
            }
        }
    }
}
