using KinkLinkBot.Configuration;
using KinkLinkBot.Domain.Models;
using KinkLinkCommon.Database;
using Microsoft.Extensions.Logging;

namespace KinkLinkBot.Services;

/// <summary>
///     Service for managing user registrations using direct PostgreSQL access
/// </summary>
public class RegistrationService
{
    private readonly QueriesSql _queries;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(
        string connectionString,
        ILogger<RegistrationService> logger)
    {
        _queries = new QueriesSql(connectionString);
        _logger = logger;
    }

    /// <summary>
    ///     Registers a new user account or returns existing if already registered
    /// </summary>
    public async Task<RegistrationResponse> RegisterUserAsync(ulong discordId)
    {
        try
        {
            // Check if user already has accounts
            var existingAccounts = await _queries.GetAccountsByDiscordIdAsync(
                new QueriesSql.GetAccountsByDiscordIdArgs((long)discordId));

            if (existingAccounts.Count > 0)
            {
                // Return first existing account
                var account = existingAccounts[0];
                return new RegistrationResponse
                {
                    Success = true,
                    UID = account.Friendcode,
                    Secret = account.Secret
                };
            }

            // Generate new credentials
            var friendCode = GenerateFriendCode();
            var secret = GenerateSecret();

            // Create account
            await _queries.CreateAccountAsync(new QueriesSql.CreateAccountArgs(
                (long)discordId,
                friendCode,
                secret));

            // Check if account was created by verifying it exists
            var checkResult = await _queries.GetAccountByFriendCodeAsync(
                new QueriesSql.GetAccountByFriendCodeArgs(friendCode));

            if (checkResult == null)
            {
                // Friend code collision - retry
                return await RegisterUserAsync(discordId);
            }

            return new RegistrationResponse
            {
                Success = true,
                UID = friendCode,
                Secret = secret
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register user {DiscordId}", discordId);
            return new RegistrationResponse
            {
                Success = false,
                ErrorMessage = "Failed to register account"
            };
        }
    }

    /// <summary>
    ///     Removes a user's account
    /// </summary>
    public async Task<RegistrationResponse> RemoveAccount(ulong discordId, string friendCode)
    {
        try
        {
            var rowsAffected = await _queries.DeleteAccountAsync(
                new QueriesSql.DeleteAccountArgs((long)discordId, friendCode));

            return new RegistrationResponse
            {
                Success = rowsAffected == 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove account for {DiscordId}", discordId);
            return new RegistrationResponse
            {
                Success = false,
                ErrorMessage = "Failed to remove account"
            };
        }
    }

    /// <summary>
    ///     Creates a secondary UID for an existing user
    /// </summary>
    public async Task<RegistrationResponse> CreateSecondaryUID(ulong discordId)
    {
        try
        {
            // Verify user exists
            var existingAccounts = await _queries.GetAccountsByDiscordIdAsync(
                new QueriesSql.GetAccountsByDiscordIdArgs((long)discordId));

            if (existingAccounts.Count == 0)
            {
                // User doesn't exist, create new account
                return await RegisterUserAsync(discordId);
            }

            // Generate new credentials
            var friendCode = GenerateFriendCode();
            var secret = GenerateSecret();

            // Create secondary account
            await _queries.CreateAccountAsync(new QueriesSql.CreateAccountArgs(
                (long)discordId,
                friendCode,
                secret));

            // Check if account was created by verifying it exists
            var checkResult = await _queries.GetAccountByFriendCodeAsync(
                new QueriesSql.GetAccountByFriendCodeArgs(friendCode));

            if (checkResult == null)
            {
                // Collision - retry
                return await CreateSecondaryUID(discordId);
            }

            return new RegistrationResponse
            {
                Success = true,
                UID = friendCode,
                Secret = secret
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create secondary UID for {DiscordId}", discordId);
            return new RegistrationResponse
            {
                Success = false,
                ErrorMessage = "Failed to create secondary UID"
            };
        }
    }

    private static string GenerateFriendCode()
    {
        // Generate a random 12-character friend code
        var random = new Random();
        const string chars = "0123456789";
        return new string(Enumerable.Range(0, 12)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }

    private static string GenerateSecret()
    {
        // Generate a secure random 64-character secret
        return Convert.ToBase64String(
            System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
    }
}
