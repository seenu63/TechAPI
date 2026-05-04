namespace TechAPI.Middleware
{
    public class RequestLoggingMiddleware : IMiddleware
    {
        private readonly IMiddlewareElementService _middlewareElementService;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(IMiddlewareElementService middlewareElementService, ILogger<RequestLoggingMiddleware> logger)
        {
            _middlewareElementService = middlewareElementService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _logger.LogInformation("Incoming request: {Path}", context.Request.Path);

            // Skip JWT check for Swagger, OpenAPI, etc.
            var path = context.Request.Path.Value;
            if (path.StartsWith("/swagger") || path.StartsWith("/openapi") || path == "/" || path.StartsWith("/User/login"))
            {
                await next(context);
                return;
            }

            bool isValid = await _middlewareElementService.LoginRequestAsync(context);

            if (!isValid)
            {
                return; // Response already written in LoginRequestAsync
            }

            await next(context);
            _logger.LogInformation("Response status: {StatusCode}", context.Response.StatusCode);
        }


    }
}

