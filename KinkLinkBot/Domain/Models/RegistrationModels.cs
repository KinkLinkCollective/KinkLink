using System.Text.Json.Serialization;

namespace KinkLinkBot.Domain.Models;

/// <summary>
///     Request to register a new user account
/// </summary>
public class RegistrationRequest
{
    [JsonPropertyName("discord_id")]
    public ulong DiscordId { get; init; }

    [JsonPropertyName("friend_code")]
    public string UID { get; init; } = string.Empty;
}

/// <summary>
///     Response from registration endpoint
/// </summary>
public class RegistrationResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("UID")]
    public string? UID { get; init; }

    [JsonPropertyName("secret")]
    public string? Secret { get; init; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }
}

/// <summary>
///     API error response
/// </summary>
public class ApiErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; init; } = string.Empty;
}
