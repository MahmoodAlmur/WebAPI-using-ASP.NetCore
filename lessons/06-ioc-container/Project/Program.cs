using Entities;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddTransient<IPayment, CardPayment>();
builder.Services.AddTransient<IPayment, GooglePayment>();
builder.Services.AddTransient<ICustomer, Customer>();


//builder.Services.AddTransient<IOperation, Operation>();
//builder.Services.AddScoped<IOperation, Operation>();
builder.Services.AddSingleton<IOperation, Operation>();


builder.Services.AddControllers();


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/operation", (IOperation first, IOperation second) =>
 $"first = {first.Id.ToString()[..8]} second = {second.Id.ToString()[..8]}");



app.MapControllers();

app.Run();
