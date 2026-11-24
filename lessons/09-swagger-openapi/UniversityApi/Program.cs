using System.Text.Json.Serialization;
using UniversityApi.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.ConfigureHttpJsonOptions(options =>
{
 options.SerializerOptions.NumberHandling =
 JsonNumberHandling.Strict;
});


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<InMemoryData>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "University API v1");
    });

}

app.MapControllers();

app.Run();