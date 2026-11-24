using Microsoft.AspNetCore.Mvc;
using UniversityApi.Data;
using UniversityApi.Dtos.Requests;
using UniversityApi.Dtos.Responses;
using UniversityApi.Dtos.Mappers;
using UniversityApi.Models;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(InMemoryData data) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<StudentResponse>>(StatusCodes.Status200OK)]
    public ActionResult<List<StudentResponse>> GetAll()
    {
        return Ok(data.Students.ToResponseList());
    }

    [HttpGet("{id}")]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<StudentResponse> GetById(int id)
    {
        Student? student = data.Students.FirstOrDefault(s => s.Id == id);

        if (student == null)
            return NotFound();

        return Ok(student.ToResponse());
    }

    [HttpGet("{id}/courses")]
    [ProducesResponseType<List<CourseResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<CourseResponse>> GetCourses(int id)
    {
        Student? student = data.Students.FirstOrDefault(s => s.Id == id);

        if (student == null)
            return NotFound();

        List<int> courseIds = data.Enrollments
            .Where(e => e.StudentId == id)
            .Select(e => e.CourseId)
            .ToList();

        List<Course> courses = data.Courses
            .Where(c => courseIds.Contains(c.Id))
            .ToList();

        return Ok(courses.ToResponseList());
    }

    [HttpPost]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<StudentResponse> Create(CreateStudentRequest request)
    {
        bool emailExists = data.Students.Any(s => s.Email == request.Email);

        if (emailExists)
            return Conflict("A student with this email already exists.");

        // The request came through Model Binding. The server decides Id and CreatedAt.
        Student student = new Student
        {
            Id = data.Students.Count == 0 ? 1 : data.Students.Max(s => s.Id) + 1,
            Name = request.Name,
            Email = request.Email,
            Password = request.Password,
            CreatedAt = DateTime.UtcNow
        };

        data.Students.Add(student);

        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student.ToResponse());
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, UpdateStudentRequest request)
    {
        Student? student = data.Students.FirstOrDefault(s => s.Id == id);

        if (student == null)
            return NotFound();

        student.Name = request.Name;
        student.Email = request.Email;

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Delete(int id)
    {
        Student? student = data.Students.FirstOrDefault(s => s.Id == id);

        if (student == null)
            return NotFound();

        bool hasEnrollments = data.Enrollments.Any(e => e.StudentId == id);

        if (hasEnrollments)
            return Conflict("The student is enrolled in courses. Remove the enrollments first.");

        data.Students.Remove(student);

        return NoContent();
    }
}
