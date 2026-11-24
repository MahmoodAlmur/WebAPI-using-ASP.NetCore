namespace UniversityApi.Dtos.Requests;

// What the client sends to create a student.
public record CreateStudentRequest(string Name, string Email, string Password);
