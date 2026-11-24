namespace UniversityApi.Dtos.Requests;

// What the client sends to log in.
public record LoginRequest(string Email, string Password);
