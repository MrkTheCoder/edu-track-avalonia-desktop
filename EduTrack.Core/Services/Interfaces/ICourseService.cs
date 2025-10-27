using EduTrack.Core.Models;

namespace EduTrack.Core.Services.Interfaces
{
    public interface ICourseService : IBaseService<Course>
    {
        Task<IEnumerable<Course>> SearchByNameAsync(string name, bool exactMatch = false);
        Task<IEnumerable<Course>> SearchByDescriptionAsync(string description, bool exactMatch = false);
    }
}
