using KinkLinkServer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KinkLinkServer.Extensions;

public static class MetricsExtensions
{
    public static IServiceCollection AddKinkLinkMetrics(this IServiceCollection services)
    {
        services.AddSingleton<IMetricsService, MetricsService>();
        return services;
    }
}