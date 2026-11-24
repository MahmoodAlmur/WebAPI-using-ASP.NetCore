using Microsoft.AspNetCore.Mvc;
using UniversityApi.Data;
using UniversityApi.Models;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/enrollments")]
[Produces("application/json")]
[Tags("Enrollments")]
public class EnrollmentsController(InMemoryData data) : ControllerBase
{
    
    [HttpGet(Name = "GetAllEnrollments")]
    [EndpointSummary("Get all enrollments")]
    [EndpointDescription("Returns the full list of enrollments. The list is empty when there are none.")]
    [ProducesResponseType<List<Enrollment>>(StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    public ActionResult<List<Enrollment>> GetAll()
    {
        return Ok(data.Enrollments);
    }

    
    [HttpGet("{id}", Name = "GetEnrollment")]
    [EndpointSummary("Get one enrollment")]
    [EndpointDescription("Returns the enrollment with the given id, or 404 when it does not exist.")]
    [ProducesResponseType<Enrollment>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public ActionResult<Enrollment> GetById(int id)
    {
        Enrollment? enrollment = data.Enrollments.FirstOrDefault(e => e.Id == id);

        if (enrollment == null)
            return NotFound();

        return Ok(enrollment);
    }

    
    [HttpPost(Name = "EnrollStudent")]
    [EndpointSummary("Enroll a student in a course")]
    [EndpointDescription("Creates an enrollment. 404 when the student or the course does not exist, 409 when the student is already enrolled.")]
    [Consumes("application/json")]
    [ProducesResponseType<Enrollment>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
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

    
    [HttpDelete("{id}", Name = "DeleteEnrollment")]
    [EndpointSummary("Remove an enrollment")]
    [EndpointDescription("Deletes the enrollment with the given id. 404 when it does not exist.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public IActionResult Delete(int id)
    {
        Enrollment? enrollment = data.Enrollments.FirstOrDefault(e => e.Id == id);

        if (enrollment == null)
            return NotFound();

        data.Enrollments.Remove(enrollment);

        return NoContent();
    }
}
