using System.Diagnostics;
using KinkLinkServer.Services;

namespace KinkLinkServer.Infrastructure;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(RequestDelegate next, IMetricsService metricsService, ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var method = context.Request.Method;
            var route = GetRouteTemplate(context) ?? context.Request.Path;
            var statusCode = context.Response.StatusCode;
            var durationMs = stopwatch.ElapsedMilliseconds;

            _metricsService.RecordHttpRequest(method, route, statusCode, durationMs);

            _logger.LogDebug("Request {Method} {Route} completed with status {StatusCode} in {Duration}ms",
                method, route, statusCode, durationMs);
        }
    }

    private static string? GetRouteTemplate(HttpContext context)
    {
        if (context.GetEndpoint() is { } endpoint)
        {
            return endpoint.DisplayName;
        }
        return null;
    }
}