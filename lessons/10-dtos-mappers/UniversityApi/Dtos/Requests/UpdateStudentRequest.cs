namespace UniversityApi.Dtos.Requests;

// What the client sends to update a student.
public record UpdateStudentRequest(string Name, string Email);
