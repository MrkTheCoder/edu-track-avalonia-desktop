using EduTrack.Core.Models;
using FluentValidation;
using FluentValidation.Validators;

namespace EduTrack.Core.Validations
{
    public class StudentValidator : AbstractValidator<Student>
    {
        public StudentValidator()
        {
            RuleFor(s => s.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(s => s.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

            RuleFor(s => s.Email)
                .NotEmpty().WithMessage("Email is required.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.")
                // The `EmailValidationMode.Net4xRegex` is deprecated. Although it is obsolete, 
                // it provides more thorough validation compared to other options. 
                // For more details, see: https://docs.fluentvalidation.net/en/latest/built-in-validators.html#email-validator
#pragma warning disable CS0618 // Type or member is obsolete
                // TODO: If a future update to FluentValidation resolves this issue, revisit this line.
                .EmailAddress(EmailValidationMode.Net4xRegex).WithMessage("Invalid email format.");
#pragma warning restore CS0618 // Type or member is obsolete

            RuleFor(s => s.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .Must(dob =>
                {
                    var age = DateTime.Today.Year - dob.Year;
                    if (dob.Date > DateTime.Today.AddYears(-age)) --age;
                    return age is >= 15 and <= 80;
                })
                .WithMessage("Student age must be between 15 and 80 years.");

        }
    }
}
