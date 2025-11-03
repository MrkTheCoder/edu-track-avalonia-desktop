using EduTrack.App.Services;
using EduTrack.Core.Interfaces;
using EduTrack.Core.Models;
using EduTrack.Core.Services.Interfaces;
using EduTrack.Core.Validations;
using EduTrack.Data.Context;
using EduTrack.Data.Repositories;
using EduTrack.IntegrationTests.Fixtures;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EduTrack.IntegrationTests.Services
{
    [Collection("IntegrationTests")]
    public class StudentServiceTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IStudentService _service;
        private readonly EduTrackDbContext _context;
        private readonly IntegrationTestFixture _fixture;

        public StudentServiceTests(IntegrationTestFixture fixture)
        {
            // Arrange Fixture
            _fixture = fixture;
            _context = _fixture.Base.Context;

            // Arrange shared dependencies for all test
            IStudentRepository repository = new StudentRepository(_context);
            IValidator<Student> validator = new StudentValidator();
            ILogger<StudentService> logger = _fixture.Base.LoggerFactory.CreateLogger<StudentService>();

            _service = new StudentService(repository, validator, logger);
        }

        // Initializes StudentService with in-memory EF context + logger + validator

        #region Setup

        private Student CreateValidStudent(string? email = null)
        {
            return new Student
            {
                FirstName = "John",
                LastName = "Doe",
                Email = email ?? $"john{Guid.NewGuid():N}@example.com",
                DateOfBirth = new DateTime(2000, 5, 12)
            };
        }

        #endregion

        // Verify happy-path CRUD behaviors (Add, GetById, Update, Remove, Search).

        #region Successful operations
        [Fact]
        public async Task AddAsync_ValidStudent_ShouldPersistAndReturnId()
        {
            //Arrange
            var created = CreateValidStudent("student.fullname@mail.com");

            //Act
            await _service.AddAsync(created);
            var retrieved = await _context.Students.FindAsync(created.Id);

            //Assert
            created.Id.Should().BeGreaterThan(0);
            retrieved.Should().NotBeNull();
            retrieved.Id.Should().Be(created.Id);
            retrieved.FirstName.Should().Be("John");
        }

        [Fact]
        public async Task GetByIdAsync_ExistingStudent_ShouldReturnStudent()
        {
            // Arrange
            var student = _context.Students.First();

            // Act
            var found = await _service.GetByIdAsync(student.Id);

            // Assert
            found.Should().NotBeNull();
            found.Id.Should().Be(student.Id);
            found.FirstName.Should().Be(student.FirstName);
        }

        [Fact]
        public async Task UpdateAsync_ExistingStudent_ShouldModifyFields()
        {
            // Arrange
            var student = _context.Students.First();
            var studentName = student.FirstName;
            var newEmail = "new.email.for.you@email.com";
            student.Email = newEmail;

            // Act
            await _service.UpdateAsync(student);
            var retrieved = await _context.Students.FindAsync(student.Id);
            
            // Assert
            retrieved.Should().NotBeNull();
            retrieved.Email.Should().Be(newEmail);
            retrieved.FirstName.Should().Be(studentName);
        }

        [Fact]
        public async Task RemoveByIdAsync_ExistingStudent_ShouldDeleteRecord()
        {
            // Arrange
            var student = _context.Students.First();
            var count = _context.Students.ToList().Count;

            // Act
            await _service.RemoveByIdAsync(student.Id);
            var retrieved = await _context.Students.FindAsync(student.Id);

            // Assert
            retrieved.Should().BeNull();
            _context.Students.Count().Should().BeLessThan(count);
            _context.Students.Count().Should().Be(count - 1);
        }
        
        [Fact]
        public async Task SearchByNameAsync_MatchingName_ShouldReturnResults()
        {
            // Arrange
            var targetName = _context.Students.First().FirstName[..3];

            // Act
            var results = await _service.SearchByNameAsync("", targetName);

            // Assert
            results.Should().NotBeEmpty();
            results.Should().OnlyContain(s => s.FirstName.Contains(targetName, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        // Simulate duplicate email, missing student, and service failures.

        #region Exception cases

        #endregion

        // Ensure FluentValidation rules trigger correctly (e.g., invalid email, age, empty fields).

        #region Validation tests

        #endregion

        // Provide small helpers like CreateValidStudent(), AddStudentAsync(), etc.

        #region Utility / Helpers

        // Each test may re-seed fresh data if needed:
        private async Task ResetAsync() => await _fixture.Base.ResetDatabaseAsync();

        #endregion
    }
}
