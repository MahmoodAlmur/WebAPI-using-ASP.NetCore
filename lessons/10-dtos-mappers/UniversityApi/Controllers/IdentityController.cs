using Microsoft.AspNetCore.Mvc;
using UniversityApi.Data;
using UniversityApi.Dtos.Requests;
using UniversityApi.Dtos.Responses;
using UniversityApi.Models;

namespace UniversityApi.Controllers;

// For explanation only: the password is stored and compared as plain text, with no hashing and no token.
[ApiController]
[Route("api/identity")]
public class IdentityController(InMemoryData data) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        Student? student = data.Students
            .FirstOrDefault(s => s.Email == request.Email && s.Password == request.Password);

        if (student == null)
            return Unauthorized("Wrong email or password.");

        return Ok(new LoginResponse(student.Id, student.Name, "Logged in."));
    }
}
