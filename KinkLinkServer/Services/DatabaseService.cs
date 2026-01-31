using KinkLinkCommon.Database;
using KinkLinkCommon.Domain;
using KinkLinkCommon.Domain.Enums;
using KinkLinkServer.Domain;
using System.Linq;

namespace KinkLinkServer.Services;

/// <summary>
///     Provides methods for interacting with the underlying PostgreSQL database from the server perspective.
///     While this covers authentication, the user functionality is currently integrated directly with a discord bot,
///     as a result, no direct account management should be included on the server
/// </summary>
public class DatabaseService
{
    // Injected
    private readonly ILogger<DatabaseService> _logger;
    private readonly string _connectionString;

    // Generated queries from sqlc
    private AuthSql _auth = null!;
    private PairsSql _pairs = null!;
    private UsersSql _users = null!;
    /// <summary>
    ///     Creates a new DatabaseService with the provided connection string and logger
    /// </summary>
    public DatabaseService(
        Configuration config,
        ILogger<DatabaseService> logger)
    {
        _connectionString = config.DatabaseConnectionString;
        _logger = logger;

        _auth = new AuthSql(_connectionString);
        _pairs = new PairsSql(_connectionString);
        _users = new UsersSql(_connectionString);
    }

    /// <summary>
    ///     Gets the user UIDs associated with the user account.
    /// </summary>
    public async Task<DBSecretAuthResult> AuthenticateUser(string secret)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(secret))
            {
                _logger.LogWarning("Authentication attempted with null or empty secret");
                return new DBSecretAuthResult
                {
                    Status = DBAuthenticationStatus.Unauthorized,
                    Uids = new()
                };
            }

            var profiles = await _auth.ListUIDsForSecretAsync(new(secret));
            var uidList = profiles.Select(row => row.Uid).ToList();
            if (uidList.Count > 0)
            {
                return new DBSecretAuthResult
                {
                    Status = DBAuthenticationStatus.Authorized,
                    Uids = uidList
                };
            }
            else
            {
                _logger.LogWarning("Authentication failed: invalid secret format");
                return new DBSecretAuthResult
                {
                    Status = DBAuthenticationStatus.Unauthorized,
                    Uids = new()
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed with unexpected error");
            return new DBSecretAuthResult
            {
                Status = DBAuthenticationStatus.UnknownError,
                Uids = new()
            };
        }
    }

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

            var result = await _auth.LoginAsync(new(uid, secret));
            if (result == null)
            {
                _logger.LogWarning("Authentication failed: Unauthorized secret and UID doesn't match");
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
    ///     Creates an empty set of permissions between sender and target friend codes
    /// </summary>
    public async Task<DBPairResult> CreatePermissions(string userUID, string targetUID)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userUID) || string.IsNullOrWhiteSpace(targetUID))
            {
                _logger.LogWarning("CreatePairRequest failed: invalid UIDs provided");
                return DBPairResult.PairUIDDoesNotExist;
            }

            if (userUID == targetUID)
            {
                _logger.LogWarning("CreatePairRequest failed: user cannot pair with themselves");
                return DBPairResult.PairUIDDoesNotExist;
            }

            // Get user IDs from UIDs using Profiles table
            var userProfiles = await _pairs.GetAllPairsForProfileAsync(new(userUID));
            var targetProfiles = await _pairs.GetAllPairsForProfileAsync(new(targetUID));

            if (!userProfiles.Any())
            {
                _logger.LogWarning("CreatePairRequest failed: user UID {UserUID} not found", userUID);
                return DBPairResult.PairUIDDoesNotExist;
            }

            if (!targetProfiles.Any())
            {
                _logger.LogWarning("CreatePairRequest failed: target UID {TargetUID} not found", targetUID);
                return DBPairResult.PairUIDDoesNotExist;
            }

            var userId = userProfiles.First().Id;
            var targetId = targetProfiles.First().Id;

            // Check if pair already exists
            var existingPairs = await _pairs.GetAllPairsForProfileAsync(new(userUID));
            var existingPair = existingPairs.FirstOrDefault(p =>
                (p.Id == userId && p.PairId == targetId) ||
                (p.Id == targetId && p.PairId == userId));

            if (existingPair != default)
            {
                if (existingPair.Id == userId && existingPair.PairId == targetId)
                {
                    _logger.LogInformation("CreatePairRequest: onesided pair already exists from {UserUID} to {TargetUID}", userUID, targetUID);
                    return DBPairResult.OnesidedPairExists;
                }
                else
                {
                    _logger.LogInformation("CreatePairRequest: users already paired {UserUID} <-> {TargetUID}", userUID, targetUID);
                    return DBPairResult.Paired;
                }
            }

            // Create new pair with no permissions (all false)
            var newPair = await _pairs.AddPairAsync(new()
            {
                Id = userId,
                PairId = targetId,
            });

            if (newPair != null)
            {
                _logger.LogInformation("CreatePairRequest: successfully created pair from {UserUID} to {TargetUID}", userUID, targetUID);
                return DBPairResult.PairCreated;
            }

            _logger.LogError("CreatePairRequest: failed to create pair from {UserUID} to {TargetUID}", userUID, targetUID);
            return DBPairResult.UnknownError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePairRequest failed with unexpected error");
            return DBPairResult.UnknownError;
        }
    }

    /// <summary>
    ///     Updates a set of permissions between sender and target friend codes
    /// </summary>
    public async Task<DBPairResult> UpdatePermissions(
        string senderFriendCode,
        string targetFriendCode,
        UserPermissions permissions)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(senderFriendCode) || string.IsNullOrWhiteSpace(targetFriendCode))
            {
                _logger.LogWarning("UpdatePermissions failed: invalid friend codes provided");
                return DBPairResult.PairUIDDoesNotExist;
            }

            if (senderFriendCode == targetFriendCode)
            {
                _logger.LogWarning("UpdatePermissions failed: cannot update permissions with self");
                return DBPairResult.PairUIDDoesNotExist;
            }

            // Get user IDs from UIDs using existing pairs
            var senderPairs = await _pairs.GetAllPairsForProfileAsync(new(senderFriendCode));
            var targetPairs = await _pairs.GetAllPairsForProfileAsync(new(targetFriendCode));

            if (!senderPairs.Any())
            {
                _logger.LogWarning("UpdatePermissions failed: sender UID {SenderUID} not found", senderFriendCode);
                return DBPairResult.PairUIDDoesNotExist;
            }

            if (!targetPairs.Any())
            {
                _logger.LogWarning("UpdatePermissions failed: target UID {TargetUID} not found", targetFriendCode);
                return DBPairResult.PairUIDDoesNotExist;
            }

            var senderId = senderPairs.First().Id;
            var targetId = targetPairs.First().Id;

            // Check if pair exists in sender->target direction
            var existingPairs = await _pairs.GetAllPairsForProfileAsync(new(senderFriendCode));
            var existingPair = existingPairs.FirstOrDefault(p =>
                p.Id == senderId && p.PairId == targetId);

            if (existingPair == default)
            {
                _logger.LogWarning("UpdatePermissions failed: no existing pair from {SenderUID} to {TargetUID}", senderFriendCode, targetFriendCode);
                return DBPairResult.NoOp;
            }

            // Update permissions using the provided permissions
            var updateResult = await _pairs.UpdatePairPermissionsAsync(new()
            {
                ToggleTimerLocks = permissions.ToggleTimerLocks,
                TogglePermanentLocks = permissions.TogglePermanentLocks,
                ToggleGarbler = permissions.ToggleGarbler,
                LockGarbler = permissions.LockGarbler,
                ToggleChannels = permissions.ToggleChannels,
                LockChannels = permissions.LockChannels,

                ApplyGag = permissions.ApplyGag,
                LockGag = permissions.LockGag,
                UnlockGag = permissions.UnlockGag,
                RemoveGag = permissions.RemoveGag,

                ApplyWardrobe = permissions.ApplyWardrobe,
                LockWardrobe = permissions.LockWardrobe,
                UnlockWardrobe = permissions.UnlockWardrobe,
                RemoveWardrobe = permissions.RemoveWardrobe,

                ApplyMoodles = permissions.ApplyMoodles,
                LockMoodles = permissions.LockMoodles,
                UnlockMoodles = permissions.UnlockMoodles,
                RemoveMoodles = permissions.RemoveMoodles,
                Id = senderId,
                PairId = targetId
            });

            if (updateResult != null)
            {
                _logger.LogInformation("UpdatePermissions: successfully updated permissions from {SenderUID} to {TargetUID}", senderFriendCode, targetFriendCode);
                return DBPairResult.Success;
            }

            _logger.LogError("UpdatePermissions: failed to update permissions from {SenderUID} to {TargetUID}", senderFriendCode, targetFriendCode);
            return DBPairResult.UnknownError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdatePermissions failed with unexpected error");
            return DBPairResult.UnknownError;
        }
    }

    /// <summary>
    ///     Gets permissions for a relationship
    /// </summary>
    public async Task<Pair?> GetPermissions(string userUID, string targetUID)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userUID) || string.IsNullOrWhiteSpace(targetUID))
            {
                _logger.LogWarning("GetPermissions failed: invalid UIDs provided");
                return null;
            }

            if (userUID == targetUID)
            {
                _logger.LogWarning("GetPermissions failed: cannot get permissions with self");
                return null;
            }

            // Get all pairs for the user
            var userPairs = await _pairs.GetAllPairsForProfileAsync(new(userUID));

            if (!userPairs.Any())
            {
                _logger.LogWarning("GetPermissions failed: user UID {UserUID} not found", userUID);
                return null;
            }

            // Find the specific pair relationship
            var targetPair = userPairs.FirstOrDefault(p => p.PairUid == targetUID);

            if (targetPair == default)
            {
                _logger.LogInformation("GetPermissions: no permissions found from {UserUID} to {TargetUID}", userUID, targetUID);
                return null;
            }

            // Create UserPermissions from the pair data
            var userPermissions = new Pair
            {
                ApplyGag = targetPair.ApplyGag,
                LockGag = targetPair.LockGag,
                UnlockGag = targetPair.UnlockGag,
                RemoveGag = targetPair.RemoveGag,
                ApplyWardrobe = targetPair.ApplyWardrobe,
                LockWardrobe = targetPair.LockWardrobe,
                UnlockWardrobe = targetPair.UnlockWardrobe,
                RemoveWardrobe = targetPair.RemoveWardrobe
            };

            _logger.LogDebug("GetPermissions: retrieved permissions from {UserUID} to {TargetUID}", userUID, targetUID);
            return userPermissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPermissions failed with unexpected error");
            return null;
        }
    }

    /// <summary>
    ///     Deletes a permissions relationship
    /// </summary>
    public async Task<DBPairResult> DeletePermissions(string userUID, string targetUID)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userUID) || string.IsNullOrWhiteSpace(targetUID))
            {
                _logger.LogWarning("DeletePermissions failed: invalid UIDs provided");
                return DBPairResult.PairUIDDoesNotExist;
            }

            if (userUID == targetUID)
            {
                _logger.LogWarning("DeletePermissions failed: cannot delete permissions with self");
                return DBPairResult.PairUIDDoesNotExist;
            }

            // Get user IDs from UIDs using existing pairs
            var userPairs = await _pairs.GetAllPairsForProfileAsync(new(userUID));
            var targetPairs = await _pairs.GetAllPairsForProfileAsync(new(targetUID));

            if (!userPairs.Any())
            {
                _logger.LogWarning("DeletePermissions failed: user UID {UserUID} not found", userUID);
                return DBPairResult.PairUIDDoesNotExist;
            }

            if (!targetPairs.Any())
            {
                _logger.LogWarning("DeletePermissions failed: target UID {TargetUID} not found", targetUID);
                return DBPairResult.PairUIDDoesNotExist;
            }

            var userId = userPairs.First().Id;
            var targetId = targetPairs.First().Id;

            // Check if pair exists before attempting deletion
            var existingPairs = await _pairs.GetAllPairsForProfileAsync(new(userUID));
            var existingPair = existingPairs.FirstOrDefault(p =>
                p.Id == userId && p.PairId == targetId);

            if (existingPair == default)
            {
                _logger.LogWarning("DeletePermissions failed: no existing pair from {UserUID} to {TargetUID}", userUID, targetUID);
                return DBPairResult.NoOp;
            }

            // Delete the pair
            var deleteResult = await _pairs.RemovePairAsync(new()
            {
                Id = userId,
                PairId = targetId
            });

            if (deleteResult != null)
            {
                _logger.LogInformation("DeletePermissions: successfully deleted pair from {UserUID} to {TargetUID}", userUID, targetUID);
                return DBPairResult.Success;
            }

            _logger.LogError("DeletePermissions: failed to delete pair from {UserUID} to {TargetUID}", userUID, targetUID);
            return DBPairResult.UnknownError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeletePermissions failed with unexpected error");
            return DBPairResult.UnknownError;
        }
    }

    /// <summary>
    ///     Helper method to get user ID from UID (friend code)
    /// </summary>
    private async Task<int?> GetUserIdFromUid(string uid)
    {
        try
        {
            var profiles = await _pairs.GetAllPairsForProfileAsync(new(uid));
            return profiles.Any() ? profiles.First().Id : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserIdFromUid failed for UID {UID}", uid);
            return null;
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

    /// <summary>
    ///     Gets all permissions for a user
    /// </summary>
    public async Task<List<TwoWayPermissions>> GetAllPermissions(string userUID)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userUID))
            {
                _logger.LogWarning("GetAllPermissions failed: invalid UID provided");
                return new List<TwoWayPermissions>();
            }

            var allPairs = await _pairs.GetAllPairsForProfileAsync(new(userUID));
            var result = new List<TwoWayPermissions>();

            foreach (var pair in allPairs)
            {
                // Convert from GetAllPairsForProfileRow to Pair model
                var pairModel = new Pair(
                    pair.Id,
                    pair.PairId,
                    pair.Expires,
                    pair.ToggleTimerLocks,
                    pair.TogglePermanentLocks,
                    pair.ToggleGarbler,
                    pair.LockGarbler,
                    pair.ToggleChannels,
                    pair.LockChannels,
                    pair.ApplyGag,
                    pair.LockGag,
                    pair.UnlockGag,
                    pair.RemoveGag,
                    pair.ApplyWardrobe,
                    pair.LockWardrobe,
                    pair.UnlockWardrobe,
                    pair.RemoveWardrobe,
                    pair.ApplyMoodles,
                    pair.LockMoodles,
                    pair.UnlockMoodles,
                    pair.RemoveMoodles
                );

                var permission = new TwoWayPermissions(userUID, pair.PairUid, pairModel, new());
                result.Add(permission);
            }

            _logger.LogDebug("GetAllPermissions: retrieved {Count} permissions for UID {UserUID}", result.Count, userUID);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllPermissions failed with unexpected error");
            return new List<TwoWayPermissions>();
        }
    }
}
