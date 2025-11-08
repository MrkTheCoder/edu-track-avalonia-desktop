using EduTrack.App.Services;
using EduTrack.Core.Enums;
using EduTrack.Core.Interfaces;
using EduTrack.Core.Models;
using EduTrack.Core.Services.Interfaces;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using EduTrack.Core.Exceptions;
using FluentValidation.Results;
using ValidationException = EduTrack.Core.Exceptions.ValidationException;

namespace EduTrack.Tests.Services
{
    public class StudentServiceTests
    {
        private readonly Mock<IStudentRepository> _repoMock;
        private readonly Mock<IValidator<Student>> _validatorMock;
        private readonly Mock<ILogger<StudentService>> _loggerMock;
        private readonly IStudentService _service;

        public StudentServiceTests()
        {
            _repoMock = new Mock<IStudentRepository>();
            _validatorMock = new Mock<IValidator<Student>>();
            _loggerMock = new Mock<ILogger<StudentService>>();

            // Default validator behavior: valid student
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Student>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
            _service = new StudentService(_repoMock.Object, _validatorMock.Object, _loggerMock.Object);
        }

        #region Setup

        private static Student CreateValidStudent(int id, string? email = null) 
            => new()
            {
                Id= id,
                FirstName = "John",
                LastName = "Doe",
                Email = email ?? $"john{Guid.NewGuid():N}@example.com",
                DateOfBirth = new DateTime(2009, 5, 13)
            };

        #endregion

        // ✅ SUCCESSFUL OPERATIONS
        #region Successful operations

        [Fact]
        [Trait("StudentService_UnitTests", "Happy Path")]
        public async Task GetByIdAsync_ExistingStudent_ShouldReturnStudentOnce()
        {
            // Arrange
            var student = CreateValidStudent(1);
            _repoMock.Setup(r => r.GetByIdAsync(student.Id)).ReturnsAsync(student);

            // Act
            var retrieved = await _service.GetByIdAsync(student.Id);

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.GetByIdAsync(student.Id), Times.Once);
            //  Testing Repository State
            retrieved.Should().NotBeNull();
            retrieved.Should().BeEquivalentTo(student);
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Happy Path")]
        public async Task GetAllAsync_ShouldReturnAllStudentsOnce()
        {
            // Arrange
            var allStudents = new List<Student>
            {
                new() { FirstName = "Alice", LastName = "Martin" },
                new() { FirstName = "Mark", LastName = "Roberts" },
                new() { FirstName = "Marcus", LastName = "Marley" }
            };
            _repoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(allStudents);

            // Act
            var results = await _service.GetAllAsync();

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            //  Testing Repository State
            results.Should().NotBeEmpty();
            results.Should().HaveCount(3);
            results.Should().BeEquivalentTo(allStudents);
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Happy Path")]
        public async Task AddAsync_ValidStudent_ShouldCallRepositoryAddAsyncOnce()
        {
            // Arrange
            var student = CreateValidStudent(1);

            // Act
            await _service.AddAsync(student);

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.AddAsync(It.Is<Student>(s => s == student)), Times.Once);
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Happy Path")]
        public async Task RemoveByIdAsync_ExistingStudent_ShouldInvokeRepositoryRemoveByIdAsyncOnce()
        {
            // Arrange
            var student = CreateValidStudent(3);
            _repoMock.Setup(r => r.GetByIdAsync(student.Id)).ReturnsAsync(student);

            // Act
            await _service.RemoveByIdAsync(student.Id);

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(v => v.RemoveByIdAsync(student.Id), Times.Once);
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Happy Path")]
        public async Task UpdateAsync_ExistingStudent_ShouldInvokeRepositoryUpdateAsyncOnce()
        {
            // Arrange
            var student = CreateValidStudent(2);
            _repoMock.Setup(r => r.GetByIdAsync(student.Id)).ReturnsAsync(student);

            // Act
            await _service.UpdateAsync(student);

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.UpdateAsync(It.Is<Student>(s => s.Id == student.Id)), Times.Once);
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Happy Path")]
        public async Task SearchByNameAsync_MatchingFirstNames_ShouldReturnFilteredStudentsOnce()
        {
            // Arrange
            var allStudents = new List<Student>
            {
                new() { FirstName = "Alice", LastName = "Smith" },
                new() { FirstName = "Alicia", LastName = "Jones" },
                new() { FirstName = "Bob", LastName = "Aliberti" }
            };
            // Sets up a mock FindAsync() that captures the LINQ expression built by
            // the service and applies it to this in-memory student list.
            SetupFindAsync(allStudents);

            // Act
            var results = await _service.SearchByNameAsync("", "Ali");

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(v => v.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()), Times.Once);
            //  Testing Repository State
            results.Should().NotBeNull();
            results.Should().HaveCount(2);
            results.Should().OnlyContain(s => s.FirstName.StartsWith("Ali"));
            results.Should().OnlyContain(s => s.FirstName == "Alice" || s.FirstName == "Alicia");
        }

        [Theory]
        [Trait("StudentService_UnitTests", "Happy Path")]
        [InlineData(null)]  // Tests the null case
        [InlineData("")]    // Tests the empty string case
        [InlineData(" ")]   // Tests the whitespace case
        [InlineData("\t")]  // Tests the tab case
        public async Task SearchByNameAsync_MatchingLastNamesWithNullOrEmptyFirstName_ShouldFiltersByLastNameOnlyOnce(string? firstNameCriteria)
        {
            // Arrange
            var allStudents = new List<Student>
            {
                new() { FirstName = "Alice", LastName = "Martin" }, 
                new() { FirstName = "Alicia", LastName = "Marley" },
                new() { FirstName = "Marcus", LastName = "Roberts" }
            };
            // Sets up a mock FindAsync() that captures the LINQ expression built by
            // the service and applies it to this in-memory student list.
            SetupFindAsync(allStudents);

            // Act
            var results = await _service.SearchByNameAsync("Mar", firstNameCriteria);

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()), Times.Once);
            //  Testing Repository State
            results.Should().NotBeEmpty();
            results.Should().HaveCount(2);
            results.Should().OnlyContain(s => s.LastName.StartsWith("Mar"));
            results.Should().OnlyContain(s => s.LastName.StartsWith("Martin") || s.LastName.StartsWith("Marley"));
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Happy Path")]
        public async Task SearchByNameAsync_MatchingFirstAndLastNames_ShouldFiltersByBothOnce()
        {
            // Arrange
            var allStudents = new List<Student>
            {
                new() { FirstName = "Alice", LastName = "Martin" },
                new() { FirstName = "Mark", LastName = "Roberts" },
                new() { FirstName = "Marcus", LastName = "Marley" }
            };
            // Sets up a mock FindAsync() that captures the LINQ expression built by
            // the service and applies it to this in-memory student list.
            SetupFindAsync(allStudents);

            // Act
            var results = await _service.SearchByNameAsync("Mar", "Mar");

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()), Times.Once);
            //  Testing Repository State
            results.Should().NotBeEmpty();
            results.Should().HaveCount(1);
            results.Should().OnlyContain(s => s.FirstName.StartsWith("Mar") && s.LastName.StartsWith("Mar"));
        }

        [Theory]
        [Trait("StudentService_UnitTests", "Happy Path")]
        [InlineData("john", 2, new[] { "John Smith", "Johnny Doe" })]   // Partial Match Case: First name contains "john"
        [InlineData("n.s", 1, new[] { "John Smith" })]                  // Partial Match Case: First and Last name contain "n.s"
        [InlineData("john.smith@mail.com", 1, new[] { "John Smith" })]  // Full Match Case: On email address
        public async Task SearchByEmailAsync_VariousMatchingCriteria_ShouldFilterResultsOnce(
                string emailCriteria,
                int expectedCount,
                string[] expectedStudentFullNames)
        {
            // Arrange
            var allStudents = new List<Student>
            {
                new() { FirstName = "John", LastName = "Smith", Email = "john.smith@mail.com" }, 
                new() { FirstName = "Johnny", LastName = "Doe", Email = "johnny.doe@mail.com" },
                new() { FirstName = "Jane", LastName = "Wick", Email = "jane.wick@mail.com" }
            };
            // Sets up a mock FindAsync() that captures the LINQ expression built by
            // the service and applies it to this in-memory student list.
            SetupFindAsync(allStudents);

            // Act
            var results = await _service.SearchByEmailAsync(emailCriteria);

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()), Times.Once);
            //  Testing Repository State
            results.Should().NotBeEmpty();
            results.Select(s => $"{s.FirstName} {s.LastName}")
                .Should()
                .HaveCount(expectedCount)
                .And
                .Contain(expectedStudentFullNames);
        }

        [Theory]
        [Trait("StudentService_UnitTests", "Happy Path")]
        [MemberData(nameof(DateSearchTestData))]    // All Date Exists
        public async Task SearchByDateOfBirthAsync_VariousMatchingCriteria_ShouldFilterResultsOnce(
            DateSearchType searchType,
            DateTime searchDate,
            int expectedCount)
        {
            // Arrange
            var allStudents = new List<Student>
            {
                new Student { FirstName = "John", LastName = "Smith", 
                    DateOfBirth = new DateTime(2000, 3, 10) },
                new Student { FirstName = "Alice", LastName = "Johnson", 
                    DateOfBirth = new DateTime(2000, 1, 15) },
                new Student { FirstName = "Emily", LastName = "Davis", 
                    DateOfBirth = new DateTime(2001, 10, 5) },
                new Student { FirstName = "Michael", LastName = "Wilson", 
                    DateOfBirth = new DateTime(1998, 11, 25) }
            };
            // Sets up a mock FindAsync() that captures the LINQ expression built by
            // the service and applies it to this in-memory student list.
            SetupFindAsync(allStudents);

            // Act
            var results = await _service.SearchByDateOfBirthAsync(searchDate, searchType);

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()), Times.Once);
            //  Testing Repository State
            results.Should().NotBeEmpty();
            results.Should().HaveCount(expectedCount);
        }

        #endregion

        // EXPECTED CASES
        #region EXPECTED CASES

        [Fact]
        [Trait("StudentService_UnitTests", "Boundary/Empty Repo")]
        public async Task GetAllAsync_WhenRepositoryIsEmpty_ShouldReturnEmptyListOnce()
        {
            // Arrange
            _repoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Student>());

            // Act
            var results = await _service.GetAllAsync();

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            //  Testing Repository State
            results.Should().NotBeNull();
            results.Should().BeEmpty();
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Boundary/Empty Repo")]
        public async Task SearchByNameAsync_WhenRepositoryIsEmpty_ShouldReturnEmptyListOnce()
        {
            // Arrange
            var allStudents = new List<Student>();
            // Sets up a mock FindAsync() that captures the LINQ expression built by
            // the service and applies it to this in-memory student list.
            SetupFindAsync(allStudents);

            // Act
            var results = await _service.SearchByNameAsync("xyz");

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()), Times.Once);
            //  Testing Repository State
            results.Should().NotBeNull();
            results.Should().BeEmpty();
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Expected/No Match")]
        public async Task SearchByNameAsync_NonExistentStudent_ShouldReturnEmptyListOnce()
        {
            // Arrange
            var allStudents = new List<Student>
            {
                new() { FirstName = "John", LastName = "Smith" },
                new() { FirstName = "Johnny", LastName = "Doe" }
            };
            // Sets up a mock FindAsync() that captures the LINQ expression built by
            // the service and applies it to this in-memory student list.
            SetupFindAsync(allStudents);

            // Act
            var results = await _service.SearchByNameAsync("xyz");

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()), Times.Once);
            //  Testing Repository State
            results.Should().NotBeNull();
            results.Should().BeEmpty();
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Expected/No Match")]
        public async Task SearchByEmailAsync_NonExistentEmail_ReturnsEmptyListOnce()
        {
            // Arrange
            var allStudents = new List<Student>
            {
                new() { FirstName = "John", LastName = "Smith", Email = "john.smith@mail.com" },
                new() { FirstName = "Jane", LastName = "Wick", Email = "jane.wick@mail.com" }
            };
            // Sets up a mock FindAsync() that captures the LINQ expression built by
            // the service and applies it to this in-memory student list.
            SetupFindAsync(allStudents);

            // Act
            var results = await _service.SearchByEmailAsync("xyz");

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()), Times.Once);
            //  Testing Repository State
            results.Should().NotBeNull();
            results.Should().BeEmpty();
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Expected/No Match")]
        public async Task SearchByDateOfBirthAsync_NonExistentDOB_ReturnsEmptyListOnce()
        {
            // Arrange
            var allStudents = new List<Student>
            {
                new() { FirstName = "John", LastName = "Smith", DateOfBirth = DateTime.Today.AddYears(-16)},
                new() { FirstName = "Jane", LastName = "Wick", DateOfBirth = DateTime.Today.AddYears(-26) }
            };
            // Sets up a mock FindAsync() that captures the LINQ expression built by
            // the service and applies it to this in-memory student list.
            SetupFindAsync(allStudents);

            // Act
            var results = await _service.SearchByDateOfBirthAsync(DateTime.Today);

            // Assert
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()), Times.Once);
            //  Testing Repository State
            results.Should().NotBeNull();
            results.Should().BeEmpty();
        }

        #endregion

        // ❌ EXCEPTION CASES
        #region EXCEPTION CASES

        [Fact]
        [Trait("StudentService_UnitTests", "Exception Case")]
        public void Constructor_WithNullDependencies_ShouldThrowArgumentNullException()
        {
            // Arrange
            var repo = _repoMock.Object;
            var validator = _validatorMock.Object;
            var logger = _loggerMock.Object;

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new StudentService(null!, validator, logger));
            Assert.Throws<ArgumentNullException>(() => new StudentService(repo, null!, logger));
            Assert.Throws<ArgumentNullException>(() => new StudentService(repo, validator, null!));
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Exception Case")]
        public async Task GetByIdAsync_NonExistingStudent_ShouldThrowEntityNotFoundExceptionOnce()
        {
            // Arrange
            var allStudents = new List<Student>
            {
                new() { Id = 1, FirstName = "John", LastName = "Smith" },
                new() { Id = 2, FirstName = "Johnny", LastName = "Doe" }
            };
            var nonExistentId = 999;
            _repoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => allStudents.FirstOrDefault(s => s.Id == id));

            // Act
            var act = async () => await _service.GetByIdAsync(nonExistentId);

            // Assert
            //  Testing Repository State
            await act.Should()
                .ThrowAsync<EntityNotFoundException>()
                .WithMessage($"*{nonExistentId}*");
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.GetByIdAsync(nonExistentId), Times.Once);
            _repoMock.VerifyNoOtherCalls();
            // ✅ Verify that the logger recorded an error
            VerifyLogErrorContains($"Student with ID '{nonExistentId}' not found.");
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Exception Case")]
        public async Task AddAsync_DuplicateEmail_ShouldThrowDuplicateEntityException()
        {
            // Arrange
            var existingStudent = CreateValidStudent(1, "a@b.c");
            var newStudent = CreateValidStudent(2, existingStudent.Email);
            var allStudents = new List<Student> { existingStudent };
            // Sets up a mock ExistsAsync() that captures the LINQ expression built by
            // the service and applies it to this in-memory student list.
            SetupExistsAsync(allStudents);

           // Act
           var act = async () => await _service.AddAsync(newStudent);

            // Assert
            //  Testing Repository State
            await act.Should()
                .ThrowAsync<DuplicateEntityException>()
                .WithMessage($"*{newStudent.Email}*");
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.ExistsAsync(It.IsAny<Expression<Func<Student, bool>>>()));
            _repoMock.Verify(r => r.AddAsync(newStudent), Times.Never);
            _repoMock.VerifyNoOtherCalls();
            // ✅ Verify that the logger recorded an error
            VerifyLogErrorContains($"Student with same email `{newStudent.Email}` exists.");
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Exception Case")]
        public async Task AddAsync_InvalidStudent_ShouldThrowValidationException()
        {
            // Arrange
            var newStudent = CreateValidStudent(1);
            var invalidResult = new FluentValidation.Results.ValidationResult(new[]
            {
                new ValidationFailure("Email", "Invalid email")
            });
            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<Student>(), default))
                .ReturnsAsync(invalidResult);

            // Act
            var act = async () => await _service.AddAsync(newStudent);

            // Assert
            //  Testing Repository State
            await act.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage($"*Validation failed*");
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.AddAsync(newStudent), Times.Never);
            _repoMock.VerifyNoOtherCalls();
            // ✅ Verify that the logger recorded an error
            VerifyLogErrorContains("Validation failed");
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Exception Case")]
        public async Task UpdateAsync_NonExistingStudent_ShouldThrowEntityNotFoundException()
        {
            // Arrange
            var newStudent = CreateValidStudent(404);
            _repoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Student?)null);

            // Act
            var act = async () => await _service.UpdateAsync(newStudent);

            // Assert
            //  Testing Repository State
            await act.Should()
                .ThrowAsync<EntityNotFoundException>()
                .WithMessage($"*{newStudent.Id}*");
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(newStudent), Times.Never);
            _repoMock.VerifyNoOtherCalls();
            // ✅ Verify that the logger recorded an error
            VerifyLogErrorContains($"Student with ID '{newStudent.Id}' not found");
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Exception Case")]
        public async Task UpdateAsync_RepositoryThrows_ShouldThrowServiceException()
        {
            // Arrange
            var student = CreateValidStudent(1);
            _repoMock.Setup(r => r.GetByIdAsync(student.Id)).ReturnsAsync(student);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Student>())).ThrowsAsync(new Exception("Update failed"));

            // Act
            var act = async () => await _service.UpdateAsync(student);

            // Assert
            await act.Should()
                .ThrowAsync<ServiceException>()
                .WithMessage("*Failed to update student*");
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Student>()), Times.Once);
            // ✅ Verify that the logger recorded an error
            VerifyLogErrorContains("Error updating student");
        }

        [Fact]
        [Trait("StudentService_UnitTests", "Exception Case")]
        public async Task RemoveByIdAsync_RepositoryThrows_ShouldThrowServiceException()
        {
            // Arrange
            var student = CreateValidStudent(1);
            _repoMock.Setup(r => r.GetByIdAsync(student.Id)).ReturnsAsync(student);
            _repoMock.Setup(r => r.RemoveByIdAsync(student.Id)).ThrowsAsync(new Exception("Remove failed"));

            // Act
            var act = async () => await _service.RemoveByIdAsync(student.Id);

            // Assert
            await act.Should()
                .ThrowAsync<ServiceException>()
                .WithMessage("*Failed to remove student*");
            //  Testing Repository Behavior
            _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _repoMock.Verify(r => r.RemoveByIdAsync(It.IsAny<int>()), Times.Once);
            // ✅ Verify that the logger recorded an error
            VerifyLogErrorContains($"Error removing student id '{student.Id}");
        }

        #endregion

        // Utilities / Helpers
        #region Utilities / Helpers
        
        private void SetupFindAsync(List<Student> allStudents)
        {
            // Create a variable to capture the predicate passed by the service
            Expression<Func<Student, bool>>? capturedPredicate = null;
            _repoMock
                // 1. Set up the method, still using Expression<Func<...>>
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                // 2. Use Callback to capture the argument passed by the service
                .Callback<Expression<Func<Student, bool>>>(p => capturedPredicate = p)
                // 3. Use Returns to execute the captured predicate against the mock data
                .ReturnsAsync(() =>
                {
                    // Check if the predicate was captured successfully
                    if (capturedPredicate == null)
                        // Throw an exception to indicate a test setup/service logic failure
                        throw new InvalidOperationException("The repository predicate was not successfully captured by the Callback. This indicates a failure in the service or mock setup.");

                    // Compile the captured expression and apply it to the mock data
                    var compiledFilter = capturedPredicate.Compile();
                    return allStudents.Where(compiledFilter);
                });
        }

        private void SetupExistsAsync(List<Student> allStudents)
        {
            Expression<Func<Student, bool>>? capturedPredicate = null;
            _repoMock
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                .Callback<Expression<Func<Student, bool>>>(p => capturedPredicate = p)
                .ReturnsAsync(() =>
                {
                    if (capturedPredicate == null)
                        throw new InvalidOperationException("The repository predicate was not successfully captured by the Callback. This indicates a failure in the service or mock setup.");

                    var compiledFilter = capturedPredicate.Compile();
                    return allStudents.Any(compiledFilter);
                });
        }

        private void VerifyLogErrorContains(string expectedMessage)
        {
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        public static IEnumerable<object[]> DateSearchTestData
            => new List<object[]>
            {
                new Object[] {DateSearchType.Equal, new DateTime(2000,3,10), 1},
                new Object[] {DateSearchType.Before, new DateTime(2000,3,9), 2},
                new Object[] {DateSearchType.YearOnly, new DateTime(2000, 1, 1), 2},
                new Object[] {DateSearchType.After, new DateTime(1999, 5, 12), 3}
            };

        #endregion
    }
}
