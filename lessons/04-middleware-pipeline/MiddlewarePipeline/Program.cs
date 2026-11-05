
using MiddlewarePipeline;

var builder = WebApplication.CreateBuilder(args);




var app = builder.Build();

app.Use(async (HttpContext context, RequestDelegate next) =>
{
    await context.Response.WriteAsync("Middleware 1 Before next()\n");
    await next(context);
    await context.Response.WriteAsync("Middleware 1 After next()\n");
});

app.Use(async (HttpContext context, RequestDelegate next) =>
{
    await context.Response.WriteAsync("Middleware 2 Before next()\n");
    int num = 2;
    
    if(num == 2)
    {
        await next(context);
    }
    await context.Response.WriteAsync("Middleware 2 After next()\n");
});


app.UseNumberedMiddleware(3);

app.UseNumberedMiddleware(4);

app.UseNumberedMiddleware(5);


// built-in middlewares
// app.UseExceptionHandler();
// app.UseStatusCodePages();
// app.UseStaticFiles();
// app.UseAuthentication();
// app.UseAuthorization();



app.Run();
