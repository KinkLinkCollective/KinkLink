using KinkLinkCommon.Database;
using KinkLinkCommon.Domain;
using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Domain.Enums.Permissions;
using KinkLinkServer.Domain;
using KinkLinkServer.Domain.Interfaces;
using KinkLinkServer.Domain.Shared;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace KinkLinkServer.Services;

/// <summary>
///     Provides methods for interacting with the underlying PostgreSQL database
/// </summary>
public class DatabaseService : IDatabaseService
{
    // Injected
    private readonly ILogger<DatabaseService> _logger;
    private readonly string _connectionString;

    // Generated queries from sqlc
    private QueriesSql _queries = null!;

    /// <summary>
    ///     Creates a new DatabaseService with the provided connection string and logger
    /// </summary>
    public DatabaseService(
        string connectionString,
        ILogger<DatabaseService> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
        _queries = new QueriesSql(connectionString);
    }

    /// <summary>
    ///     Gets a user entry from the accounts table by secret
    /// </summary>
    public async Task<string?> GetFriendCodeBySecret(string secret)
    {
        try
        {
            var result = await _queries.GetFriendCodeBySecretAsync(new QueriesSql.GetFriendCodeBySecretArgs(secret));
            return result?.Friendcode;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to get user with secret {Secret}, {Exception}", secret, e.Message);
            return null;
        }
    }

    /// <summary>
    ///     Creates an empty set of permissions between sender and target friend codes
    /// </summary>
    public async Task<DatabaseResultEc> CreatePermissions(string senderFriendCode, string targetFriendCode)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var transactionQueries = QueriesSql.WithTransaction(transaction);

        try
        {
            // Check if target exists
            var targetExists = await transactionQueries.CheckFriendCodeExistsAsync(new QueriesSql.CheckFriendCodeExistsArgs(targetFriendCode));
            if (targetExists == null)
            {
                return DatabaseResultEc.NoSuchFriendCode;
            }

            // Create permissions
            var rowsAffected = await transactionQueries.CreatePermissionsAsync(new QueriesSql.CreatePermissionsArgs(senderFriendCode, targetFriendCode));

            if (rowsAffected == 0)
            {
                // Already friends
                return DatabaseResultEc.AlreadyFriends;
            }

            // Check if they added us back
            var permissionsToUs = await transactionQueries.GetPermissionsAsync(new QueriesSql.GetPermissionsArgs(targetFriendCode, senderFriendCode));

            await transaction.CommitAsync();
            return permissionsToUs != null ? DatabaseResultEc.Success : DatabaseResultEc.Pending;
        }
        catch (Exception e)
        {
            _logger.LogError("[CreatePermissions] {Error}", e);
            await transaction.RollbackAsync();
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     Updates a set of permissions between sender and target friend codes
    /// </summary>
    public async Task<DatabaseResultEc> UpdatePermissions(
        string senderFriendCode,
        string targetFriendCode,
        UserPermissions permissions)
    {
        try
        {
            var rowsAffected = await _queries.UpdatePermissionsAsync(new QueriesSql.UpdatePermissionsArgs(
                (int)permissions.Primary,
                (int)permissions.Speak,
                (int)permissions.Elevated,
                senderFriendCode,
                targetFriendCode));

            return rowsAffected == 1 ? DatabaseResultEc.Success : DatabaseResultEc.NoOp;
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                "Unable to update {FriendCode}'s permissions for {TargetFriendCode}, {Exception}",
                senderFriendCode, targetFriendCode, e.Message);
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     Gets permissions for a relationship
    /// </summary>
    public async Task<UserPermissions?> GetPermissions(string friendCode, string targetFriendCode)
    {
        try
        {
            var result = await _queries.GetPermissionsAsync(new QueriesSql.GetPermissionsArgs(friendCode, targetFriendCode));

            if (result == null)
                return null;

            return new UserPermissions(
                (PrimaryPermissions2)result.Value.Primarypermissions,
                (SpeakPermissions2)result.Value.Speakpermissions,
                (ElevatedPermissions)result.Value.Elevatedpermissions);
        }
        catch (Exception e)
        {
            _logger.LogError("[GetPermissions] {Error}", e);
            return null;
        }
    }

    /// <summary>
    ///     Gets all permissions for a user
    /// </summary>
    public async Task<List<TwoWayPermissions>> GetAllPermissions(string friendCode)
    {
        try
        {
            var results = await _queries.GetAllPermissionsAsync(new QueriesSql.GetAllPermissionsArgs(friendCode));
            var permissions = new List<TwoWayPermissions>();

            foreach (var row in results)
            {
                var primary = (PrimaryPermissions2)row.Primarypermissionsto;
                var speak = (SpeakPermissions2)row.Speakpermissionsto;
                var elevated = (ElevatedPermissions)row.Elevatedpermissionsto;

                if (row.Primarypermissionsfrom == null)
                {
                    permissions.Add(new TwoWayPermissions(
                        friendCode,
                        row.Targetfriendcode,
                        primary, speak, elevated));
                }
                else
                {
                    permissions.Add(new TwoWayPermissions(
                        friendCode,
                        row.Targetfriendcode,
                        primary, speak, elevated,
                        (PrimaryPermissions2)row.Primarypermissionsfrom,
                        (SpeakPermissions2)row.Speakpermissionsfrom,
                        (ElevatedPermissions)row.Elevatedpermissionsfrom));
                }
            }

            return permissions;
        }
        catch (Exception e)
        {
            _logger.LogError("[GetAllPermissions] {Error}", e);
            return [];
        }
    }

    /// <summary>
    ///     Deletes a permissions relationship
    /// </summary>
    public async Task<DatabaseResultEc> DeletePermissions(string senderFriendCode, string targetFriendCode)
    {
        try
        {
            var rowsAffected = await _queries.DeletePermissionsAsync(new QueriesSql.DeletePermissionsArgs(senderFriendCode, targetFriendCode));
            return rowsAffected == 1 ? DatabaseResultEc.Success : DatabaseResultEc.NoOp;
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                "Unable to delete {FriendCode}'s permissions for {TargetFriendCode}, {Exception}",
                senderFriendCode, targetFriendCode, e.Message);
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     Admin function to create an account
    /// </summary>
    public async Task<DatabaseResultEc> AdminCreateAccount(ulong discord, string friendCode, string secret)
    {
        try
        {
            await _queries.CreateAccountAsync(new QueriesSql.CreateAccountArgs((long)discord, friendCode, secret));

            // Check if account was created by verifying it exists
            var checkResult = await _queries.GetAccountByFriendCodeAsync(new QueriesSql.GetAccountByFriendCodeArgs(friendCode));
            return checkResult != null ? DatabaseResultEc.Success : DatabaseResultEc.FriendCodeAlreadyExists;
        }
        catch (Exception e)
        {
            _logger.LogError("[AdminCreateAccount] Failed to create account for discord {Discord}, {Error}", discord, e);
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     Gets accounts for a discord user
    /// </summary>
    public async Task<List<KinkLinkServer.Domain.Shared.Account>?> AdminGetAccounts(ulong discord)
    {
        try
        {
            var results = await _queries.GetAccountsByDiscordIdAsync(new QueriesSql.GetAccountsByDiscordIdArgs((long)discord));

            return results
                .Select(r => new KinkLinkServer.Domain.Shared.Account(r.Friendcode, r.Secret, r.Admin))
                .ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(
                "[AdminGetAccounts] Failed to get accounts for discord {Discord}, {Error}",
                discord, e);
            return null;
        }
    }

    /// <summary>
    ///     Updates an account's friend code
    /// </summary>
    public async Task<DatabaseResultEc> AdminUpdateAccount(ulong discord, string oldFriendCode, string newFriendCode)
    {
        try
        {
            var rowsAffected = await _queries.UpdateFriendCodeAsync(new QueriesSql.UpdateFriendCodeArgs(newFriendCode, (long)discord, oldFriendCode));
            return rowsAffected == 1 ? DatabaseResultEc.Success : DatabaseResultEc.NoSuchFriendCode;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "[AdminUpdateAccount] Failed to update friend code for discord {Discord}, {Error}",
                discord, e);
            return DatabaseResultEc.Unknown;
        }
    }

    /// <summary>
    ///     Deletes an account
    /// </summary>
    public async Task<DatabaseResultEc> AdminDeleteAccount(ulong discord, string friendCode)
    {
        try
        {
            var rowsAffected = await _queries.DeleteAccountAsync(new QueriesSql.DeleteAccountArgs((long)discord, friendCode));
            return rowsAffected == 1 ? DatabaseResultEc.Success : DatabaseResultEc.NoSuchFriendCode;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "[AdminDeleteAccount] Failed to delete account for discord {Discord}, {Error}",
                discord, e);
            return DatabaseResultEc.Unknown;
        }
    }
}
