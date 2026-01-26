using System.Text.Json.Serialization;

namespace KinkLinkBot.Domain.Models;

/// <summary>
///     Request to register a new user account
/// </summary>
public class RegistrationRequest
{
    [JsonPropertyName("discord_id")]
    public ulong DiscordId { get; init; }
}

/// <summary>
///     Response from registration endpoint
/// </summary>
public class RegistrationResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("secret")]
    public string? Secret { get; init; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }
}

///     <summary>
///     Response from profile regiration endpoint
/// </summary>
public class ProfileResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("uid")]
    public string? UID { get; init; }

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
