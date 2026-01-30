using Discord.WebSocket;
using KinkLinkBot.Configuration;
using KinkLinkBot.Domain.Models;
using KinkLinkCommon.Database;
using Microsoft.Extensions.Logging;

namespace KinkLinkBot.Services;

/// <summary>
///     Service for managing user registrations using direct PostgreSQL access
///     These are all handled by a series of embeded messages that only the users
///     can see. There is no need for any slash commands to be used.
/// </summary>
public class RegistrationService
{
    private readonly AuthSql _auth;
    private readonly UsersSql _users;
    private readonly ProfilesSql _profiles;
    private readonly ILogger<RegistrationService> _logger;
    private readonly DiscordSocketClient _client;
    private readonly BotConfiguration _config;

    public RegistrationService(
        ILogger<RegistrationService> logger,
        DiscordSocketClient client,
        BotConfiguration config)
    {
        var connectionString = config.DbConnectionString;
        _auth = new AuthSql(connectionString);
        _users = new UsersSql(connectionString);
        _profiles = new ProfilesSql(connectionString);
        _logger = logger;
        _client = client;
        _config = config;
    }

    /// <summary>
    ///     Registers a new user account or returns existing if already registered
    /// </summary>
    public async Task<RegistrationResponse> RegisterUserAccount(ulong discordId)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _users.SelectUserbyDiscordIdAsync(new((long)discordId));

            if (existingUser.HasValue)
            {
                // User already exists, check if they're banned
                if (existingUser.Value.Banned == true)
                {
                    return new RegistrationResponse
                    {
                        Success = false,
                        ErrorMessage = "Your account has been banned. Please contact an administrator."
                    };
                }

                // Return existing user info
                return new RegistrationResponse
                {
                    Success = true,
                    Secret = existingUser.Value.SecretKey
                };
            }

            // Create new user
            var secret = GenerateSecret();
            var newUser = await _users.RegisterNewUserAsync(new(
                DiscordId: (long)discordId,
                SecretKey: secret
            ));

            if (newUser.HasValue)
            {
                _logger.LogInformation($"New user registered: Discord ID {discordId}, User ID {newUser.Value.Id}");

                return new RegistrationResponse
                {
                    Success = true,
                    Secret = newUser.Value.SecretKey
                };
            }
            else
            {
                _logger.LogError($"Failed to register new user with Discord ID {discordId}");
                return new RegistrationResponse
                {
                    Success = false,
                    ErrorMessage = "Failed to create user account. Please try again later."
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user account for Discord ID {DiscordId}", discordId);
            return new RegistrationResponse
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred during registration."
            };
        }
    }

    /// <summary>
    ///     Removes a user's account
    /// </summary>
    public async Task<RegistrationResponse> RemoveAccount(ulong discordId)
    {
        try
        {
            var existingUser = await _users.SelectUserbyDiscordIdAsync(new((long)discordId));

            if (!existingUser.HasValue)
            {
                return new RegistrationResponse
                {
                    Success = false,
                    ErrorMessage = "User account not found."
                };
            }

            var deletedUser = await _users.DeleteUserAccountAsync(new((long)discordId));

            if (deletedUser.HasValue)
            {
                _logger.LogInformation($"User account deleted: Discord ID {discordId}, User ID {deletedUser.Value.Id}");

                return new RegistrationResponse
                {
                    Success = true,
                    ErrorMessage = null
                };
            }
            else
            {
                _logger.LogError($"Failed to delete user account with Discord ID {discordId}");
                return new RegistrationResponse
                {
                    Success = false,
                    ErrorMessage = "Failed to delete user account. Please try again later."
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user account for Discord ID {DiscordId}", discordId);
            return new RegistrationResponse
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred while deleting your account."
            };
        }
    }

    /// <summary>
    ///     Creates a UID for the  user.
    ///     UIDs are used to maintain relative anonymity with the user accounts.
    /// </summary>
    public async Task<ProfileResponse> CreateUID(ulong discordId)
    {
        try
        {
            var existingUser = await _users.SelectUserbyDiscordIdAsync(new((long)discordId));

            if (!existingUser.HasValue)
            {
                return new ProfileResponse
                {
                    Success = false,
                    ErrorMessage = "User account not found. Please register first."
                };
            }

            if (existingUser.Value.Banned == true)
            {
                return new ProfileResponse
                {
                    Success = false,
                    ErrorMessage = "Your account has been banned. Please contact an administrator."
                };
            }

            var newUID = GenerateUID();

            var profileExists = await _profiles.ProfileExistsAsync(new(newUID));
            // This is _exceedingly unlikely_ but I'm paranoid, so just in case.
            // (Odds of this occurring are 1/3,656,158,440,062,976)
            if (profileExists?.Exists == true)
            {
                return new ProfileResponse
                {
                    Success = false,
                    ErrorMessage = "We currently have Unable to generate a unique UID. Please try again later."
                };
            }

            var newProfile = await _profiles.CreateNewUIDForUserAsync(new(
                UserId: existingUser.Value.Id,
                Uid: newUID,
                // TODO: Clean this up (allow for alias to be set during creation)
                ChatRole: null,
                Alias: null,
                Title: null,
                Description: null
            ));

            if (newProfile.HasValue)
            {
                _logger.LogInformation($"New UID created: {newUID} for Discord ID {discordId}, User ID {existingUser.Value.Id}");

                return new ProfileResponse
                {
                    Success = true,
                    UID = newUID
                };
            }
            else
            {
                _logger.LogError($"Failed to create UID {newUID} for Discord ID {discordId}");
                return new ProfileResponse
                {
                    Success = false,
                    ErrorMessage = "Failed to create UID. Please try again later."
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating UID for Discord ID {DiscordId}", discordId);
            return new ProfileResponse
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred while creating your UID."
            };
        }
    }

    public async Task<ProfileResponse> DeleteUID(ulong discordId, string UID)
    {
        try
        {
            var existingUser = await _users.SelectUserbyDiscordIdAsync(new((long)discordId));

            if (!existingUser.HasValue)
            {
                return new ProfileResponse
                {
                    Success = false,
                    ErrorMessage = "User account not found."
                };
            }

            var profileExists = await _profiles.ProfileExistsAsync(new(UID));
            if (!profileExists?.Exists == true)
            {
                return new ProfileResponse
                {
                    Success = false,
                    ErrorMessage = "UID not found."
                };
            }

            var deletedProfile = await _profiles.DeleteProfileAsync(new(UID, existingUser.Value.Id));

            if (deletedProfile.HasValue)
            {
                _logger.LogInformation($"UID {UID} deleted for Discord ID {discordId}, User ID {existingUser.Value.Id}");

                return new ProfileResponse
                {
                    Success = true,
                    ErrorMessage = null
                };
            }
            else
            {
                _logger.LogError($"Failed to delete UID {UID} for Discord ID {discordId}");
                return new ProfileResponse
                {
                    Success = false,
                    ErrorMessage = "Failed to delete UID. The UID may not belong to your account."
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting UID {UID} for Discord ID {DiscordId}", UID, discordId);
            return new ProfileResponse
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred while deleting your UID."
            };
        }
    }

    private static string GenerateUID()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var result = new char[10];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }

        return new string(result);
    }

    private static string GenerateSecret()
    {
        // Generate a secure random 64-character secret
        return Convert.ToBase64String(
            System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
    }
}
