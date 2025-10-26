using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduTrack.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Data.Repositories
{
    public class CourseRepository(DbContext context)
        :GenericRepository<Course>(context)
    {
    }
}
