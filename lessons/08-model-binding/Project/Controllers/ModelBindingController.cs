using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers;

[ApiController]
public class ModelBindingController : ControllerBase
{
    [HttpGet("/route/{id:int}/{name}")]
    public IActionResult GetModelBinding([FromRoute] int id, [FromRoute] string name)
    {
        return Ok(new { Id = id, Name = name });
    }

    [HttpGet("/query")]
    public IActionResult GetModelBindingFromQuery([FromQuery] string name, [FromQuery] int age)
    {
        return Ok(new { Name = name, Age = age });
    }

    [HttpGet("/query2")]
    public IActionResult GetModelBindingFromQuery([FromQuery] Pagination pagination)
    {
        return Ok(pagination);
    }

    [HttpPost("/body")]
    public IActionResult GetModelBindingFromBody([FromBody] Person person)
    {
        return Ok(person);
    }

    [HttpGet("/header")]
    public IActionResult GetModelBindingFromHeader([FromHeader] string theme)
    {
        return Ok(new { Theme = theme });
    }

    [HttpGet("/service")]
    public IActionResult GetModelBindingFromService([FromServices] ILogger<ModelBindingController> logger)
    {
        logger.LogInformation("Model binding from service called.");
        return Ok(new { Message = "Model binding from service successful." });
    }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}


public class Pagination
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}