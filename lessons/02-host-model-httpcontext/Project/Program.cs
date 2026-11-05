var builder = WebApplication.CreateBuilder(args);



var app = builder.Build();


app.MapGet("/info", (HttpContext context) => new
{
    Method = context.Request.Method,
    Path = context.Request.Path.Value,
    Query = context.Request.QueryString.Value,
    UserAgent = context.Request.Headers.UserAgent.ToString(),
    Ip = context.Connection.RemoteIpAddress?.ToString(),
    TraceIdentifier = context.TraceIdentifier,
    Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
});


app.MapGet("/write", async (HttpContext context) =>
{
    context.Response.StatusCode = 201;
    context.Response.Headers["X-Course"] = "ASP.NET Core";
    context.Response.ContentType = "text/plain; charset=utf-8";
    await context.Response.WriteAsync("Written directly to the response");
});


app.Run();
