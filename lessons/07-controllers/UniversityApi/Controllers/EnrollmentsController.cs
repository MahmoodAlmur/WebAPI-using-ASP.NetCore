using Microsoft.AspNetCore.Mvc;
using UniversityApi.Data;
using UniversityApi.Models;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController(InMemoryData data) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Enrollment>> GetAll()
    {
        return Ok(data.Enrollments);
    }

    [HttpGet("{id}")]
    public ActionResult<Enrollment> GetById(int id)
    {
        Enrollment? enrollment = data.Enrollments.FirstOrDefault(e => e.Id == id);

        if (enrollment == null)
            return NotFound();

        return Ok(enrollment);
    }

    [HttpPost]
    public ActionResult<Enrollment> Enroll(Enrollment request)
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

        return CreatedAtAction(nameof(GetById), new { id = enrollment.Id }, enrollment);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        Enrollment? enrollment = data.Enrollments.FirstOrDefault(e => e.Id == id);

        if (enrollment == null)
            return NotFound();

        data.Enrollments.Remove(enrollment);

        return NoContent();
    }
}
