using EduTrack.Core.Interfaces;
using EduTrack.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Data.Repositories
{
    public class CourseRepository(DbContext context)
        :GenericRepository<Course>(context), ICourseRepository
    {
    }
}
