using EduTrack.App.Services;
using EduTrack.Core.Exceptions;
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
using System.Linq.Expressions;
using ValidationException = EduTrack.Core.Exceptions.ValidationException;

namespace EduTrack.IntegrationTests.Services
{
    [Collection("IntegrationTests")]
    public class StudentServiceTests : IClassFixture<IntegrationTestFixture>
    {
        // Initializes StudentService with in-memory EF context + logger + validator
        #region Setup

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
        [Trait("IntegrationTests", "Happy Path")]
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
        [Trait("IntegrationTests", "Happy Path")]
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
        [Trait("IntegrationTests", "Happy Path")]
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
        [Trait("IntegrationTests", "Happy Path")]
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
        [Trait("IntegrationTests", "Happy Path")]
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

        [Fact]
        [Trait("IntegrationTests", "Exception Case")]
        public async Task AddAsync_DuplicateEmail_ShouldThrowDuplicateEntityException()
        {
            // Arrange
            var existing = _context.Students.First();
            var duplicate = new Student
            {
                FirstName = "NewName",
                LastName = "NewLastName",
                Email = existing.Email, // Same Email
                DateOfBirth = new DateTime(2003,11,11)
            };

            // Act
            var act = async () => await _service.AddAsync(duplicate);

            // Assert
            await act.Should()
                .ThrowAsync<DuplicateEntityException>()
                .WithMessage($"*{existing.Email}*");
        }

        [Fact]
        [Trait("IntegrationTests", "Exception Case")]
        public async Task AddAsync_InvalidStudent_ShouldThrowValidationException()
        {
            // Arrange
            var invalidStudent = new Student
            {
                FirstName = "", // invalid 
                LastName = "", // invalid
                Email = "not-an-email",
                DateOfBirth = DateTime.Today // too young (age < 15)
            };

            // Act
            var act = async () => await _service.AddAsync(invalidStudent);
            
            // Assert
            await act.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("*Validation failed*");
        }

        [Fact]
        [Trait("IntegrationTests", "Exception Case")]
        public async Task RemoveByIdAsync_NonExistingStudent_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            var invalidId = 9999;

            // Act
            var act = async () => await _service.RemoveByIdAsync(invalidId);

            // Assert
            await act.Should()
                .ThrowAsync<EntityNotFoundException>()
                .WithMessage($"*{invalidId}*");
        }

        [Fact]
        [Trait("IntegrationTests", "Exception Case")]
        public async Task UpdateAsync_NonExistingStudent_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            var nonExistStudent = new Student
            {
                FirstName = "Ghost",
                LastName = "AnotherWorld",
                Email = "ghost@somewhere.any",
                DateOfBirth = new DateTime(2008,1,1)
            };

            // Act
            var act = async() => await _service.UpdateAsync(nonExistStudent);

            // Assert
            await act.Should()
                .ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        [Trait("IntegrationTests", "Exception Case")]
        public async Task GetByIdAsync_RepositoryThrows_ShouldWrapInServiceException()
        {
            // Arrange
            var faultyRepo = new FaultyRepository();
            var validator = new StudentValidator();
            var logger = _fixture.Base.LoggerFactory.CreateLogger<StudentService>();
            var service = new StudentService(faultyRepo, validator, logger);

            // Act
            var act = async () => await service.GetByIdAsync(1);

            // Assert
            await act.Should()
                .ThrowAsync<ServiceException>()
                .WithMessage($"*Failed to retrieve student*");
        }
        #endregion
        
        // Provide small helpers like CreateValidStudent(), AddStudentAsync(), etc.
        #region Utility / Helpers

        // Each test may re-seed fresh data if needed:
        private async Task ResetAsync() => await _fixture.Base.ResetDatabaseAsync();


        private class FaultyRepository : IStudentRepository
        {
            public Task<Student?> GetByIdAsync(int id) => throw new Exception("DB connection failed");
            public Task<IEnumerable<Student>> GetAllAsync() => throw new Exception("DB connection failed");
            public Task<IEnumerable<Student>> FindAsync(Expression<Func<Student, bool>> predicate) => throw new Exception("DB connection failed");
            public Task<bool> ExistsAsync(Expression<Func<Student, bool>> predicate) => throw new Exception("DB connection failed");
            public Task AddAsync(Student entity) => throw new Exception("DB connection failed");
            public Task AddRangeAsync(IEnumerable<Student> entities) => throw new Exception("DB connection failed");
            public Task RemoveAsync(Student entity) => throw new Exception("DB connection failed");
            public Task RemoveByIdAsync(int id) => throw new Exception("DB connection failed");
            public Task RemoveRangeAsync(IEnumerable<Student> entities) => throw new Exception("DB connection failed");
            public Task RemoveAllAsync() => throw new Exception("DB connection failed");
            public Task UpdateAsync(Student entity) => throw new Exception("DB connection failed");
        }

        #endregion
    }
}
