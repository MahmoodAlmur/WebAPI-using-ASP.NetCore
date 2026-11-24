using UniversityApi.Dtos.Responses;
using UniversityApi.Models;

namespace UniversityApi.Dtos.Mappers;

public static class StudentMapper
{
    // Entity -> Response (what we return)
    public static StudentResponse ToResponse(this Student student)
    {
        return new StudentResponse(student.Id, student.Name, student.Email);
    }

    // A list of entities -> a list of Responses
    public static List<StudentResponse> ToResponseList(this IEnumerable<Student> students)
    {
        return students.Select(s => s.ToResponse()).ToList();
    }
}
