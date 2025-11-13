using EduTrack.Core.Models;
using EduTrack.Core.Validations;
using FluentAssertions;
using FluentValidation.Results;

namespace EduTrack.Tests.Validations
{
    public class StudentValidatorTests
    {
        #region Setup

        private readonly StudentValidator _validator = new();

        private static Student CreateValidStudent() => new Student
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            DateOfBirth = new DateTime(2000, 5, 12)
        };

        private ValidationResult Validate(Student student) => _validator.Validate(student);

        #endregion

        #region Happy Path

        [Fact]
        [Trait("StudentValidator_UnitTests", "Happy Path")]
        public void Validate_ValidStudent_ShouldPassValidation()
        {
            // Arrange
var student = CreateValidStudent();

            // Act
            var result = Validate(student);

            // Assert
            result.IsValid.Should().BeTrue("all required fields are valid");
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region Required Fields

        [Theory]
        [Trait("StudentValidator_UnitTests", "Required Fields")]
        [InlineData("", "last", "email@mail.com", "2000/01/01", "FirstName")]
        [InlineData("first", "", "email@mail.com", "2000/01/01", "LastName")]
        [InlineData("first", "last", "", "2000/01/01", "Email")]
        public void Validate_MissingRequiredFields_ShouldFailValidation(
            string firstName, string lastName, string email, string dob, string expectedPropertyName)
        {
            // Arrange
            var student = new Student
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                DateOfBirth = DateTime.Parse(dob)
            };

            // Act
            var result = Validate(student);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => e.PropertyName == expectedPropertyName
                                                && e.ErrorMessage.Contains("is required"));
        }

        #endregion

        #region Field Lengths

        [Fact]
        [Trait("StudentValidator_UnitTests", "Field Lengths")]
        public void Validate_FirstNameExceedsLimit_ShouldFail()
        {
            // Arrange
            var student = CreateValidStudent();
            student.FirstName = new string('A', 101);

            // Act
            var result = Validate(student);

            // Asert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(student.FirstName)
                                                && e.ErrorMessage == "First name cannot exceed 100 characters.");
        }

        [Fact]
        [Trait("StudentValidator_UnitTests", "Field Lengths")]
        public void Validate_LastNameExceedsLimit_ShouldFail()
        {
        // Arrange
        var student = CreateValidStudent();
        student.LastName = new string('B', 101);

        // Act
        var result = Validate(student);

        // Assert
        result.IsValid.Should().BeFalse("Last name cannot exceed 100 characters");
        result.Errors.Should().NotBeEmpty().And
            .Contain(e => 
                e.ErrorMessage == "Last name cannot exceed 100 characters." &&
                e.PropertyName == nameof(student.LastName));
        }

        [Fact]
        [Trait("StudentValidator_UnitTests", "Field Lengths")]
        public void Validate_EmailExceedsLimit_ShouldFail()
        {
            // Arrange
            var student = CreateValidStudent();
            student.Email = new string('E', 247) + "@mail.com";

            // Act
var result = Validate(student);

            // Assert
            result.IsValid.Should().BeFalse("Email cannot exceed 255 characters");
            result.Errors.Should().NotBeEmpty().And
                .Contain(s => s.ErrorMessage == "Email cannot exceed 255 characters." &&
                              s.PropertyName == nameof(student.Email));
        }

        #endregion

        #region Email Format

        [Theory]
        [Trait("StudentValidator_UnitTests", "Email Format")]
        [InlineData("plainaddress")]
        [InlineData("missingatsign.com")]
        [InlineData("@nouser.com")]
        [InlineData("user@.com")]
        [InlineData("user@domain")]
        public void Validate_InvalidEmailFormat_ShouldFail(string invalidEmail)
        {
            // Arrange
            var student = CreateValidStudent();
            student.Email = invalidEmail;

            // Act
            var result = Validate(student);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty().And
                .Contain(e => e.ErrorMessage == "Invalid email format." &&
                              e.PropertyName == nameof(student.Email));
        }

        #endregion

        #region Date of Birth (Age Limits)

        [Theory]
        [Trait("StudentValidator_UnitTests", "Date of Birth")]
        [InlineData(10)]  // too young
        [InlineData(85)]  // too old
        public void Validate_AgeOutOfRange_ShouldFail(int age)
        {
            // Arrange
            var student = CreateValidStudent();
            student.DateOfBirth = DateTime.Today.AddYears(-age);

            // Act
            var result = Validate(student);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "DateOfBirth" && e.ErrorMessage.Contains("between 15 and 80"));
        }

        [Fact]
        [Trait("StudentValidator_UnitTests", "Date of Birth")]
        public void Validate_ValidAgeWithinRange_ShouldPass()
        {
            // Arrange
            var student = CreateValidStudent();
            student.DateOfBirth = DateTime.Today.AddYears(-25);

            // Act
            var result = Validate(student);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region Edge Cases

        [Fact]
        [Trait("StudentValidator_UnitTests", "Edge Cases")]
        public void Validate_FieldsAtBoundaries_ShouldPassValidation()
        {
            // Arrange
            var student = CreateValidStudent();
            student.FirstName = new string('F', 100);
            student.LastName = new string('L', 100);
            student.Email = new string('E', 246) + "@mail.com";

            // Act
            var result = Validate(student);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [Trait("StudentValidator_UnitTests", "Edge Cases")]
        [InlineData(15)]    // Valid minimum age boundary
        [InlineData(80)]    // Valid maximum age boundary
        public void Validate_AgeAtBoundery_ShouldPassValidation(int age)
        {
            // Arrange
            var student = CreateValidStudent();
            student.DateOfBirth = DateTime.Today.AddYears(-age);

            // Act
            var result = Validate(student);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion
    }
}
