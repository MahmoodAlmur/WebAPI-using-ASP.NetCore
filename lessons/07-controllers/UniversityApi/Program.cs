using UniversityApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<InMemoryData>();

var app = builder.Build();

app.MapControllers();

app.Run();
