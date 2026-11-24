using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using UniversityApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// Describes numbers as plain integer/number in the OpenAPI document (instead of integer or string),
// so that Swagger UI accepts path and query values in Try it out.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});
builder.Services.AddSingleton<InMemoryData>();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "University API",
            Version = "v1",
            Description = "Students, courses and enrollments, with in-memory data."
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseCors();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "University API v1");
    });
}

app.Run();