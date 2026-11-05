namespace MiddlewarePipeline;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseNumberedMiddleware(this IApplicationBuilder  app, int number)
    {
        return app.Use(async (HttpContext context, RequestDelegate next) =>
        {
            await context.Response.WriteAsync($"Middleware {number} Before next()\n");
            await next(context);
            await context.Response.WriteAsync($"Middleware {number} After next()\n");
        });
    }
}       