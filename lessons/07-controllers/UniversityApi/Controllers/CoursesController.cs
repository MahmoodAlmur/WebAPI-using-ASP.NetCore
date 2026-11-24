using Microsoft.AspNetCore.Mvc;
using UniversityApi.Data;
using UniversityApi.Models;

namespace UniversityApi.Controllers;


[ApiController]
[Route("api/courses")]
[Produces("application/json")]
[Tags("Courses")]
public class CoursesController(InMemoryData data) : ControllerBase
{
    
    [HttpGet(Name = "GetAllCourses")]
    [EndpointSummary("Get all courses")]
    [EndpointDescription("Returns the full list of courses. The list is empty when there are none.")]
    [ProducesResponseType<List<Course>>(StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    public ActionResult<List<Course>> GetAll()
    {
        return Ok(data.Courses);
    }

    
    [HttpGet("{id}", Name = "GetCourse")]
    [EndpointSummary("Get one course")]
    [EndpointDescription("Returns the course with the given id, or 404 when it does not exist.")]
    [ProducesResponseType<Course>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public ActionResult<Course> GetById(int id)
    {
        Course? course = data.Courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
            return NotFound();

        return Ok(course);
    }

    
    [HttpGet("{id}/students", Name = "GetCourseStudents")]
    [EndpointSummary("Get the students of a course")]
    [EndpointDescription("Looks up the enrollments of the course and returns the matching students. 404 when the course does not exist.")]
    [ProducesResponseType<List<Student>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public ActionResult<List<Student>> GetStudents(int id)
    {
        Course? course = data.Courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
            return NotFound();

        List<int> studentIds = data.Enrollments
            .Where(e => e.CourseId == id)
            .Select(e => e.StudentId)
            .ToList();

        List<Student> students = data.Students
            .Where(s => studentIds.Contains(s.Id))
            .ToList();

        return Ok(students);
    }

    
    [HttpPost(Name = "CreateCourse")]
    [EndpointSummary("Create a course")]
    [EndpointDescription("Creates a course and returns it with a Location header. 409 when the title is already used.")]
    [Consumes("application/json")]
    [ProducesResponseType<Course>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    public ActionResult<Course> Create(Course course)
    {
        bool titleExists = data.Courses.Any(c => c.Title == course.Title);

        if (titleExists)
            return Conflict("A course with this title already exists.");

        course.Id = data.Courses.Count == 0 ? 1 : data.Courses.Max(c => c.Id) + 1;
        data.Courses.Add(course);

        return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
    }

    
    [HttpPut("{id}", Name = "UpdateCourse")]
    [EndpointSummary("Update a course")]
    [EndpointDescription("Replaces the title of an existing course. 404 when it does not exist.")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public IActionResult Update(int id, Course updated)
    {
        Course? course = data.Courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
            return NotFound();

        course.Title = updated.Title;

        return NoContent();
    }

    
    [HttpDelete("{id}", Name = "DeleteCourse")]
    [EndpointSummary("Delete a course")]
    [EndpointDescription("Deletes a course. 409 when the course still has enrolled students, 404 when it does not exist.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]    
    public IActionResult Delete(int id)
    {
        Course? course = data.Courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
            return NotFound();

        bool hasEnrollments = data.Enrollments.Any(e => e.CourseId == id);

        if (hasEnrollments)
            return Conflict("The course has enrolled students. Remove the enrollments first.");

        data.Courses.Remove(course);

        return NoContent();
    }
}
