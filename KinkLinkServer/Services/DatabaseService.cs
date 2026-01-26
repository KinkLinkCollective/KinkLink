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
public class DatabaseService
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
    public async Task<string?> AuthenticateUser(string secret)
    {
        // TODO: Implement the authentication for this function.
        // Hash the user secret
        // Check against the database for the user existing.
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Creates an empty set of permissions between sender and target friend codes
    /// </summary>
    public async Task<DBPairResult> CreatePairRequest(string senderUID, string targetUID)
    {
        // TODO: If there is not already a one way pair, create a new pair request 
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Updates a set of permissions between sender and target friend codes
    /// </summary>
    public async Task<DBPairResult> UpdatePermissions(
        string senderFriendCode,
        string targetFriendCode,
        UserPermissions permissions)
    {
        // TODO: If there there is a two way pair, update the permissions with the data from UserPermissions
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Gets permissions for a relationship
    /// </summary>
    public async Task<UserPermissions?> GetPermissions(string friendCode, string targetFriendCode)
    {
        // TODO: 
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Gets all permissions for a user
    /// </summary>
    public async Task<List<TwoWayPermissions>> GetAllPermissions(string friendCode)
    {
        //
        // TODO: 
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Deletes a permissions relationship
    /// </summary>
    public async Task<DBPairResult> DeletePermissions(string senderFriendCode, string targetFriendCode)
    {
        // TODO: 
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Admin function to create an account
    /// </summary>
    public async Task<DBPairResult> AdminCreateAccount(ulong discord, string friendCode, string secret)
    {
        // TODO: 
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Gets accounts for a discord user
    /// </summary>
    public async Task<List<KinkLinkServer.Domain.Shared.Account>?> AdminGetAccounts(ulong discord)
    {
        // TODO: 
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Updates an account's friend code
    /// </summary>
    public async Task<DBPairResult> AdminUpdateAccount(ulong discord, string oldFriendCode, string newFriendCode)
    {
        // TODO: 
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Deletes an account
    /// </summary>
    public async Task<DBPairResult> AdminDeleteAccount(ulong discord, string friendCode)
    {
        // TODO: 
        throw new NotImplementedException();
    }
}
