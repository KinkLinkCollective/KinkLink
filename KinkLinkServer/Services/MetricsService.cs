using KinkLinkCommon.Domain.Network.AddFriend;
using KinkLinkCommon.Domain.Enums;
using Prometheus;

namespace KinkLinkServer.Services;

public interface IMetricsService
{
    void RecordHttpRequest(string method, string route, int statusCode, double durationMs);
    void IncrementSignalRConnection(string action);
    void IncrementSignalRMessage(string handler, bool success);
    void RecordSignalRMessageDuration(string handler, double durationMs);
    void IncrementAuthentication(string action, bool success);
    void RecordAuthenticationDuration(string action, double durationMs);
    void SetActiveConnections(int count);
    void SetOnlineUsers(int count);
    void IncrementDatabaseOperation(string operation, bool success);
    void RecordDatabaseOperationDuration(string operation, double durationMs);
    void IncrementRequestLogging();
}

public class MetricsService : IMetricsService
{
    private static readonly Counter HttpRequestCounter = Metrics
        .CreateCounter("http_requests_total", "Total HTTP requests", new[] { "method", "route", "status_code" });

    private static readonly Histogram HttpRequestDuration = Metrics
        .CreateHistogram("http_request_duration_seconds", "HTTP request duration in seconds", new[] { "method", "route" });

    private static readonly Counter SignalRConnectionCounter = Metrics
        .CreateCounter("signalr_connections_total", "Total SignalR connection events", new[] { "action" });

    private static readonly Counter SignalRMessageCounter = Metrics
        .CreateCounter("signalr_messages_total", "Total SignalR messages processed", new[] { "handler", "success" });

    private static readonly Histogram SignalRMessageDuration = Metrics
        .CreateHistogram("signalr_message_duration_seconds", "SignalR message processing duration in seconds", new[] { "handler" });

    private static readonly Counter AuthenticationCounter = Metrics
        .CreateCounter("authentication_total", "Total authentication attempts", new[] { "action", "success" });

    private static readonly Histogram AuthenticationDuration = Metrics
        .CreateHistogram("authentication_duration_seconds", "Authentication duration in seconds", new[] { "action" });

    private static readonly Gauge ActiveConnectionsGauge = Metrics
        .CreateGauge("active_connections", "Number of active connections");

    private static readonly Gauge OnlineUsersGauge = Metrics
        .CreateGauge("online_users", "Number of online users");

    private static readonly Counter DatabaseOperationCounter = Metrics
        .CreateCounter("database_operations_total", "Total database operations", new[] { "operation", "success" });

    private static readonly Histogram DatabaseOperationDuration = Metrics
        .CreateHistogram("database_operation_duration_seconds", "Database operation duration in seconds", new[] { "operation" });

    private static readonly Counter RequestLoggingCounter = Metrics
        .CreateCounter("request_logging_total", "Total request logging entries");

    public void RecordHttpRequest(string method, string route, int statusCode, double durationMs)
    {
        HttpRequestCounter.WithLabels(method, route, statusCode.ToString()).Inc();
        HttpRequestDuration.WithLabels(method, route).Observe(durationMs / 1000.0);
    }

    public void IncrementSignalRConnection(string action)
    {
        SignalRConnectionCounter.WithLabels(action).Inc();
    }

    public void IncrementSignalRMessage(string handler, bool success)
    {
        SignalRMessageCounter.WithLabels(handler, success.ToString().ToLowerInvariant()).Inc();
    }

    public void RecordSignalRMessageDuration(string handler, double durationMs)
    {
        SignalRMessageDuration.WithLabels(handler).Observe(durationMs / 1000.0);
    }

    public void IncrementAuthentication(string action, bool success)
    {
        AuthenticationCounter.WithLabels(action, success.ToString().ToLowerInvariant()).Inc();
    }

    public void RecordAuthenticationDuration(string action, double durationMs)
    {
        AuthenticationDuration.WithLabels(action).Observe(durationMs / 1000.0);
    }

    public void SetActiveConnections(int count)
    {
        ActiveConnectionsGauge.Set(count);
    }

    public void SetOnlineUsers(int count)
    {
        OnlineUsersGauge.Set(count);
    }

    public void IncrementDatabaseOperation(string operation, bool success)
    {
        DatabaseOperationCounter.WithLabels(operation, success.ToString().ToLowerInvariant()).Inc();
    }

    public void RecordDatabaseOperationDuration(string operation, double durationMs)
    {
        DatabaseOperationDuration.WithLabels(operation).Observe(durationMs / 1000.0);
    }

    public void IncrementRequestLogging()
    {
        RequestLoggingCounter.Inc();
    }
}
