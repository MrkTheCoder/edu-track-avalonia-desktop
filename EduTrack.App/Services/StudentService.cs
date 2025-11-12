using EduTrack.Core.Enums;
using EduTrack.Core.Exceptions;
using EduTrack.Core.Interfaces;
using EduTrack.Core.Models;
using EduTrack.Core.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ValidationException = EduTrack.Core.Exceptions.ValidationException;

namespace EduTrack.App.Services
{
    public class StudentService(
        IStudentRepository repo, 
        IValidator<Student> validator, 
        ILogger<StudentService> logger
        )
        : IStudentService
    {
        private readonly IStudentRepository _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        private readonly IValidator<Student> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        private readonly ILogger<StudentService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Student> GetByIdAsync(int id)
        {
            try
            {
                return await EnsureStudentExistsAsync(id);
            }
            catch (Exception ex) when (ex is not EntityNotFoundException)
            {
                                           _logger.LogError(ex, "Error retrieving student by id '{Id}'", id);
                throw new ServiceException($"Failed to retrieve student {id}.", ex);
            }
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            try
            {
                return await _repo.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all students");
                throw new ServiceException("Failed to retrieve students.", ex);
            }
        }

        public async Task<IEnumerable<Student>> SearchByEmailAsync(string email)
        {
            try
            {
                return await _repo.FindAsync(s => s.Email.Contains(email));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching students by email '{email}'", email);
                throw new ServiceException("Failed to search students by email.", ex);
            }
        }

        public async Task<IEnumerable<Student>> SearchByNameAsync(string lastName, string? firstName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(firstName))
                    return await _repo.FindAsync(s => s.LastName.Contains(lastName));
                return await _repo.FindAsync(s => s.LastName.Contains(lastName) && s.FirstName.Contains(firstName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching students by name, '{name}'", $"'{(firstName ?? "null")}', '{lastName}'");
                throw new ServiceException("Failed to search students by name.", ex);
            }
        }

        public async Task<IEnumerable<Student>> SearchByDateOfBirthAsync(DateTime dateOfBirth, DateSearchType searchType = DateSearchType.Equal)
        {
            try
            {
                return searchType switch
                {
                    DateSearchType.Equal => await _repo.FindAsync(s => s.DateOfBirth.Date == dateOfBirth.Date),
                    DateSearchType.After => await _repo.FindAsync(s => s.DateOfBirth.Date > dateOfBirth.Date),
                    DateSearchType.Before => await _repo.FindAsync(s => s.DateOfBirth.Date < dateOfBirth.Date),
                    DateSearchType.YearOnly => await _repo.FindAsync(s => s.DateOfBirth.Year == dateOfBirth.Year),
                    _ => await _repo.FindAsync(s => s.DateOfBirth.Date == dateOfBirth.Date)

                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching students by date of birth '{dateOfBirth}'", dateOfBirth);
                throw new ServiceException("Failed to search students by date of birth.", ex);
            }
        }

        public async Task AddAsync(Student entity)
        {
            await ValidateEntityAsync(entity);

            try
            {
                await CheckEmailDuplicationAsync(entity.Email);
                await _repo.AddAsync(entity);
            }
            catch (Exception ex) when (ex is not DuplicateEntityException)
            {
                _logger.LogError(ex, "Error adding student, {student}", $"FN:'{(entity.FirstName ?? "null")}', LN:'{entity.LastName}', Email:`{entity.Email}`, DOB:'{entity.DateOfBirth}'");
                throw new ServiceException("Failed to add student.", ex);
            }
        }

        public async Task RemoveByIdAsync(int id)
        {
            try
            {
                await EnsureStudentExistsAsync(id);
                await _repo.RemoveByIdAsync(id);
            }
            catch (Exception ex) when (ex is not EntityNotFoundException)
            {
                _logger.LogError(ex, "Error removing student id '{Id}'", id);
                throw new ServiceException($"Failed to remove student {id}.", ex);
            }
        }

        public async Task UpdateAsync(Student entity)
        {
            await ValidateEntityAsync(entity);

            try
            {
                await EnsureStudentExistsAsync(entity.Id);
                await CheckEmailDuplicationAsync(entity.Email);
                await _repo.UpdateAsync(entity);
            }
            catch (Exception ex) when (ex is not (DuplicateEntityException or EntityNotFoundException))
            {
                _logger.LogError(ex, "Error updating student '{StudentId}'", entity.Id);
                throw new ServiceException("Failed to update student.", ex);
            }
        }

        private async Task ValidateEntityAsync(Student entity)
        {
            var validation = await _validator.ValidateAsync(entity);
            if (!validation.IsValid)
            {
                _logger.LogError("Validation failed: {ValidationErrors}", string.Join("; ", validation.Errors));
                throw new ValidationException("Validation failed: " + string.Join("; ", validation.Errors));
            }
        }

        private async Task<Student> EnsureStudentExistsAsync(int id)
        {
            var student = await _repo.GetByIdAsync(id);
            if (student == null)
            {
                _logger.LogError("Student with ID '{id}' not found.", id);
                throw new EntityNotFoundException($"Student with ID {id} not found.");
            }
            return student;
        }

        private async Task CheckEmailDuplicationAsync(string email)
        {
            if (await _repo.ExistsAsync(s => s.Email == email))
            {
                _logger.LogError("Student with same email `{email}` exists.", email);
                throw new DuplicateEntityException($"Student with same email `{email}` already exists.");
            }
        }
    }
}
