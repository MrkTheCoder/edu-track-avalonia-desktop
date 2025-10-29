using EduTrack.Core.Interfaces;
using EduTrack.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Data.Repositories
{
    public class StudentRepository(DbContext context) 
        : GenericRepository<Student>(context), IStudentRepository
    {
    }
}
