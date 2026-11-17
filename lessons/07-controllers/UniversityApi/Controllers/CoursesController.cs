using Microsoft.AspNetCore.Mvc;
using UniversityApi.Data;
using UniversityApi.Models;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(InMemoryData data) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Course>> GetAll()
    {
        return Ok(data.Courses);
    }

    [HttpGet("{id}")]
    public ActionResult<Course> GetById(int id)
    {
        Course? course = data.Courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
            return NotFound();

        return Ok(course);
    }

    [HttpGet("{id}/students")]
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

    [HttpPost]
    public ActionResult<Course> Create(Course course)
    {
        bool titleExists = data.Courses.Any(c => c.Title == course.Title);

        if (titleExists)
            return Conflict("A course with this title already exists.");

        course.Id = data.Courses.Count == 0 ? 1 : data.Courses.Max(c => c.Id) + 1;
        data.Courses.Add(course);

        return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Course updated)
    {
        Course? course = data.Courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
            return NotFound();

        course.Title = updated.Title;

        return NoContent();
    }

    [HttpDelete("{id}")]
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
