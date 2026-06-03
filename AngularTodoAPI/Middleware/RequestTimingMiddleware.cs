using System.Diagnostics; // this provides the Stopwatch class, which is used to measure the elapsed time of the HTTP request processing in milliseconds. The Stopwatch class is a high-resolution timer that can be used to accurately measure time intervals, making it ideal for performance monitoring and logging in web applications.

namespace AngularTodoAPI.Middleware
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next; // allows the middleware to call the next middleware in the pipeline after it has completed its work
        private readonly ILogger<RequestTimingMiddleware> _logger; // used to log information about the request timing, such as the HTTP method, path, status code, and elapsed time in milliseconds
        private const double SlowThresholdMs = 2000; // 2 seconds

        // The constructor takes in the next RequestDelegate and an ILogger instance, which are injected by the ASP.NET Core dependency injection system when the middleware is added to the pipeline
        public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // InvokeAsync is a required method because it is part of the IMiddleware interface. It is called for each HTTP request
        public async Task InvokeAsync(HttpContext context) // context parameter represents the current HTTP request and response, and allows the middleware to access and modify them as needed
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                var elapsedMs = sw.Elapsed.TotalMilliseconds;

                // Only log API requests to reduce noise
                if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                {
                    if (elapsedMs >= SlowThresholdMs)
                    {
                        _logger.LogWarning(
                            "SLOW HTTP {Method} {Route} responded {StatusCode} in {ElapsedMs:0.###} ms",
                            context.Request.Method,
                            context.Request.Path,
                            context.Response?.StatusCode,
                            elapsedMs);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "HTTP {Method} {Route} responded {StatusCode} in {ElapsedMs:0.###} ms",
                            context.Request.Method,
                            context.Request.Path,
                            context.Response?.StatusCode,
                            elapsedMs);
                    }
                }
            }
        }
    }
}