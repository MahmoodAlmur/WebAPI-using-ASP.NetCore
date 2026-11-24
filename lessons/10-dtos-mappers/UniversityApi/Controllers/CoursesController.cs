using Microsoft.AspNetCore.Mvc;
using UniversityApi.Data;
using UniversityApi.Dtos.Requests;
using UniversityApi.Dtos.Responses;
using UniversityApi.Dtos.Mappers;
using UniversityApi.Models;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(InMemoryData data) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<CourseResponse>>(StatusCodes.Status200OK)]
    public ActionResult<List<CourseResponse>> GetAll()
    {
        return Ok(data.Courses.ToResponseList());
    }

    [HttpGet("{id}")]
    [ProducesResponseType<CourseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<CourseResponse> GetById(int id)
    {
        Course? course = data.Courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
            return NotFound();

        return Ok(course.ToResponse());
    }

    [HttpGet("{id}/students")]
    [ProducesResponseType<List<StudentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<StudentResponse>> GetStudents(int id)
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

        return Ok(students.ToResponseList());
    }

    [HttpPost]
    [ProducesResponseType<CourseResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<CourseResponse> Create(CreateCourseRequest request)
    {
        bool titleExists = data.Courses.Any(c => c.Title == request.Title);

        if (titleExists)
            return Conflict("A course with this title already exists.");

        Course course = new Course
        {
            Id = data.Courses.Count == 0 ? 1 : data.Courses.Max(c => c.Id) + 1,
            Title = request.Title
        };

        data.Courses.Add(course);

        return CreatedAtAction(nameof(GetById), new { id = course.Id }, course.ToResponse());
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, UpdateCourseRequest request)
    {
        Course? course = data.Courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
            return NotFound();

        course.Title = request.Title;

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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
