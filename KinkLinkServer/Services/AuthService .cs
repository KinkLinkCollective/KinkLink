using KinkLinkCommon.Database;
using KinkLinkCommon.Domain.Enums;

namespace KinkLinkServer.Services;

/// <summary>
///     Provides methods for interacting with the underlying PostgreSQL database from the server perspective.
///     While this covers authentication, the user functionality is currently integrated directly with a discord bot,
///     as a result, no direct account management should be included on the server
/// </summary>
public class AuthService
{
    // Injected
    private readonly ILogger<DatabaseService> _logger;

    // Generated queries from sqlc
    private AuthSql _auth;

    /// <summary>
    ///     Creates a new DatabaseService with the provided connection string and logger
    /// </summary>
    public AuthService(AuthSql auth, ILogger<DatabaseService> logger)
    {
        _logger = logger;

        _auth = auth;
    }

    // TODO: Implement discord OAUTH and don't use the secretkey.
    /// <summary>
    ///     Gets a user entry from the accounts table by secret
    /// </summary>
    public async Task<DBAuthenticationStatus> LoginUser(string secret, string uid)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(uid))
            {
                _logger.LogWarning("Authentication attempted with null or empty secret");
                return DBAuthenticationStatus.Unauthorized;
            }

            var result = await _auth.LoginAsync(new(uid));
            if (result is not { } value || !value.IsValid)
            {
                _logger.LogWarning("Authentication failed: UID not found or missing hash data");
                return DBAuthenticationStatus.Unauthorized;
            }

            return DBAuthenticationStatus.Authorized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed with unexpected error");
            return DBAuthenticationStatus.UnknownError;
        }
    }

    /// <summary>
    ///     Helper method to validate UID format
    /// </summary>
    private bool IsValidUid(string uid)
    {
        return !string.IsNullOrWhiteSpace(uid) && uid.Length >= 3 && uid.Length <= 10;
    }

    /// <summary>
    ///     Helper method to validate secret format
    /// </summary>
    private bool IsValidSecret(string secret)
    {
        return !string.IsNullOrWhiteSpace(secret) && secret.Length >= 10 && secret.Length <= 100;
    }
}
