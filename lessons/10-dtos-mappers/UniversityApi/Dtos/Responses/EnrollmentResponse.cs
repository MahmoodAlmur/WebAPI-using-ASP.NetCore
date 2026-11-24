namespace UniversityApi.Dtos.Responses;

// The response carries the names, so the client does not need extra calls.
public record EnrollmentResponse(
    int Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseTitle,
    DateTime EnrolledAt);
