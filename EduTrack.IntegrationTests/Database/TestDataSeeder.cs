using EduTrack.Core.Models;
using EduTrack.Data.Context;

namespace EduTrack.IntegrationTests.Database
{
    public static class TestDataSeeder
    {
        public static void SeedBasicData(EduTrackDbContext context)
        {

            if (!context.Students.Any()) // avoid duplicate seeding
            {
                context.Students.AddRange(
                    new Student
                    {
                        FirstName = "Bob",
                        LastName = "Smith",
                        Email = "bob.smith@email.com",
                        DateOfBirth = new DateTime(1999, 7, 13)
                    },
                    new Student
                    {
                        FirstName = "Ava",
                        LastName = "Green",
                        Email = "ava.green@somemail.com",
                        DateOfBirth = new DateTime(2000, 5, 20)
                    },
                    new Student
                    {
                        FirstName = "Bobbi",
                        LastName = "Brown",
                        Email = "william.brown@email.com",
                        DateOfBirth = new DateTime(1974, 9, 10)
                    },
                    new Student
                    {
                        FirstName = "Alice",
                        LastName = "Johnson",
                        Email = "alice.johnson@example.com",
                        DateOfBirth = new DateTime(2006, 3, 14)
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
