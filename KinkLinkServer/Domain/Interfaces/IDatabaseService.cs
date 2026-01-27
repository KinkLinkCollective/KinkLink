using KinkLinkCommon.Domain;
using KinkLinkCommon.Domain.Enums;
using KinkLinkServer.Domain.Shared;

namespace KinkLinkServer.Domain.Interfaces;

/// <summary>
///     Provides access to the underlying PostgreSQL database
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    ///     Authenticates a user by secret key
    /// </summary>
    public Task<DBAuthenticationResult> AuthenticateUser(string secret);

    /// <summary>
    ///     Creates an empty set of permissions between sender and target friend codes
    /// </summary>
    public Task<DBPairResult> CreatePairRequest(string userUID, string targetUID);
    

    /// <summary
    ///     Updates a set of permissions between sender and target friend codes
    /// </summary>
    public Task<DBPairResult> UpdatePermissions(
        string senderFriendCode,
        string targetFriendCode,
        UserPermissions permissions);
    

    /// <summary>
    ///     Gets permissions for a relationship
    /// </summary>
    public Task<UserPermissions?> GetPermissions(string userUID, string targetUID);
    

    /// <summary>
    ///     Deletes a permissions relationship
    /// </summary>
    public Task<DBPairResult> DeletePermissions(string userUID, string targetUID);

    /// <summary>
    ///     Gets friend code by secret key
    /// </summary>
    public Task<string?> GetFriendCodeBySecret(string secret);

    /// <summary>
    ///     Creates permissions entry
    /// </summary>
    public Task<DBPermissionsResults> CreatePermissions(string userUID, string targetUID, UserPermissions permissions);

    /// <summary>
    ///     Gets all permissions for a user
    /// </summary>
    public Task<List<TwoWayPermissions>> GetAllPermissions(string userUID);
}
