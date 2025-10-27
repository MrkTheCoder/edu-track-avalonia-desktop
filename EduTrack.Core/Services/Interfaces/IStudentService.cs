using EduTrack.Core.Enums;
using EduTrack.Core.Models;

namespace EduTrack.Core.Services.Interfaces
{
    public interface IStudentService : IBaseService<Student>
    {
        Task<IEnumerable<Student>> SearchByEmailAsync(string email);
        Task<IEnumerable<Student>> SearchByNameAsync(string lastName, string? firstName = null);
        Task<IEnumerable<Student>> SearchByDateOfBirthAsync(DateTime dateOfBirth, DateSearchType searchType = DateSearchType.Equal);
    }
}
