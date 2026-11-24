using UniversityApi.Dtos.Responses;
using UniversityApi.Models;

namespace UniversityApi.Dtos.Mappers;

public static class CourseMapper
{
    // Entity -> Response (what we return)
    public static CourseResponse ToResponse(this Course course)
    {
        return new CourseResponse(course.Id, course.Title);
    }

    // A list of entities -> a list of Responses
    public static List<CourseResponse> ToResponseList(this IEnumerable<Course> courses)
    {
        return courses.Select(c => c.ToResponse()).ToList();
    }
}
