using UniversityApi.Dtos.Responses;
using UniversityApi.Models;

namespace UniversityApi.Dtos.Mappers;

public static class EnrollmentMapper
{
    // The DTO combines data from three entities, so the mapper receives all of them.
    // Entity -> Response (what we return)
    public static EnrollmentResponse ToResponse(this Enrollment enrollment, Student student, Course course)
    {
        return new EnrollmentResponse(
            enrollment.Id,
            student.Id,
            student.Name,
            course.Id,
            course.Title,
            enrollment.EnrolledAt);
    }
}
