using UniversityApi.Models;

namespace UniversityApi.Data;

public class InMemoryData
{
    public List<Student> Students = new List<Student>
    {
        new Student { Id = 1, Name = "Ahmed Ali", Email = "ahmed@example.com", Password = "ahmed123", CreatedAt = new DateTime(2026, 8, 1) },
        new Student { Id = 2, Name = "Sara Omar", Email = "sara@example.com", Password = "sara123", CreatedAt = new DateTime(2026, 8, 2) },
        new Student { Id = 3, Name = "Khaled Nasser", Email = "khaled@example.com", Password = "khaled123", CreatedAt = new DateTime(2026, 8, 3) }
    };

    public List<Course> Courses = new List<Course>
    {
        new Course { Id = 1, Title = "ASP.NET Core" },
        new Course { Id = 2, Title = "C# Basics" },
        new Course { Id = 3, Title = "SQL" }
    };

    public List<Enrollment> Enrollments = new List<Enrollment>
    {
        new Enrollment { Id = 1, StudentId = 1, CourseId = 1, EnrolledAt = new DateTime(2026, 9, 1) },
        new Enrollment { Id = 2, StudentId = 2, CourseId = 2, EnrolledAt = new DateTime(2026, 9, 2) }
    };
}
