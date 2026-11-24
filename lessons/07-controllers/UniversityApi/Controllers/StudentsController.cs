using Microsoft.AspNetCore.Mvc;
using UniversityApi.Data;
using UniversityApi.Models;

namespace UniversityApi.Controllers;


[ApiController]
[Route("api/students")]
[Produces("application/json")]
[Tags("Students")]
public class StudentsController(InMemoryData data) : ControllerBase
{
    
    [HttpGet(Name = "GetAllStudents")]
    [EndpointSummary("Get all students")]
    [EndpointDescription("Returns the full list of students. The list is empty when there are none.")]
    [ProducesResponseType<List<Student>>(StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    public ActionResult<List<Student>> GetAll()
    {
        return Ok(data.Students);
    }

    
    [HttpGet("{id}", Name = "GetStudent")]
    [EndpointSummary("Get one student")]
    [EndpointDescription("Returns the student with the given id, or 404 when it does not exist.")]
    [ProducesResponseType<Student>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public ActionResult<Student> GetById(int id)
    {
        Student? student = data.Students.FirstOrDefault(s => s.Id == id);

        if (student == null)
            return NotFound();

        return Ok(student);
    }

    
    [HttpGet("{id}/courses", Name = "GetStudentCourses")]
    [EndpointSummary("Get the courses of a student")]
    [EndpointDescription("Looks up the enrollments of the student and returns the matching courses. 404 when the student does not exist.")]
    [ProducesResponseType<List<Course>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public ActionResult<List<Course>> GetCourses(int id)
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

        return Ok(courses);
    }

    
    [HttpPost(Name = "CreateStudent")]
    [EndpointSummary("Create a student")]
    [EndpointDescription("Creates a student and returns it with a Location header. 409 when the email is already used.")]
    [Consumes("application/json")]
    [ProducesResponseType<Student>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    public ActionResult<Student> Create(Student student)
    {
        bool emailExists = data.Students.Any(s => s.Email == student.Email);

        if (emailExists)
            return Conflict("A student with this email already exists.");

        student.Id = data.Students.Count == 0 ? 1 : data.Students.Max(s => s.Id) + 1;
        data.Students.Add(student);

        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    
    [HttpPut("{id}", Name = "UpdateStudent")]
    [EndpointSummary("Update a student")]
    [EndpointDescription("Replaces the name and the email of an existing student. 404 when it does not exist.")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public IActionResult Update(int id, Student updated)
    {
        Student? student = data.Students.FirstOrDefault(s => s.Id == id);

        if (student == null)
            return NotFound();

        student.Name = updated.Name;
        student.Email = updated.Email;

        return NoContent();
    }

    
    [HttpDelete("{id}", Name = "DeleteStudent")]
    [EndpointSummary("Delete a student")]
    [EndpointDescription("Deletes a student. 409 when the student still has enrollments, 404 when it does not exist.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
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
