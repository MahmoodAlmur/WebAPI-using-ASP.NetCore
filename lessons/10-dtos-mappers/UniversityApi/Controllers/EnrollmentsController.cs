using Microsoft.AspNetCore.Mvc;
using UniversityApi.Data;
using UniversityApi.Dtos.Requests;
using UniversityApi.Dtos.Responses;
using UniversityApi.Dtos.Mappers;
using UniversityApi.Models;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController(InMemoryData data) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<EnrollmentResponse>>(StatusCodes.Status200OK)]
    public ActionResult<List<EnrollmentResponse>> GetAll()
    {
        List<EnrollmentResponse> result = data.Enrollments
            .Select(MapWithNames)
            .ToList();

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<EnrollmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EnrollmentResponse> GetById(int id)
    {
        Enrollment? enrollment = data.Enrollments.FirstOrDefault(e => e.Id == id);

        if (enrollment == null)
            return NotFound();

        return Ok(MapWithNames(enrollment));
    }

    [HttpPost]
    [ProducesResponseType<EnrollmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<EnrollmentResponse> Enroll(EnrollRequest request)
    {
        Student? student = data.Students.FirstOrDefault(s => s.Id == request.StudentId);

        if (student == null)
            return NotFound($"Student {request.StudentId} was not found.");

        Course? course = data.Courses.FirstOrDefault(c => c.Id == request.CourseId);

        if (course == null)
            return NotFound($"Course {request.CourseId} was not found.");

        bool alreadyEnrolled = data.Enrollments
            .Any(e => e.StudentId == request.StudentId && e.CourseId == request.CourseId);

        if (alreadyEnrolled)
            return Conflict("The student is already enrolled in this course.");

        Enrollment enrollment = new Enrollment
        {
            Id = data.Enrollments.Count == 0 ? 1 : data.Enrollments.Max(e => e.Id) + 1,
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrolledAt = DateTime.UtcNow
        };

        data.Enrollments.Add(enrollment);

        return CreatedAtAction(nameof(GetById), new { id = enrollment.Id }, enrollment.ToResponse(student, course));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        Enrollment? enrollment = data.Enrollments.FirstOrDefault(e => e.Id == id);

        if (enrollment == null)
            return NotFound();

        data.Enrollments.Remove(enrollment);

        return NoContent();
    }

    // Finds the student and the course of an enrollment, then maps it.
    private EnrollmentResponse MapWithNames(Enrollment enrollment)
    {
        Student student = data.Students.First(s => s.Id == enrollment.StudentId);
        Course course = data.Courses.First(c => c.Id == enrollment.CourseId);

        return enrollment.ToResponse(student, course);
    }
}
