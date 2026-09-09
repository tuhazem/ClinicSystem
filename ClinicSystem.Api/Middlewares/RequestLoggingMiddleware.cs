using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ClinicSystem.Api.Middlewares;

public class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        logger.LogInformation("HTTP {Method} {Path} started", request.Method, request.Path);

        await next(context);

        stopwatch.Stop();
        var response = context.Response;

        logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
            request.Method,
            request.Path,
            response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
