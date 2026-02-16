using KinkLinkCommon.Database;
using KinkLinkCommon.Domain;
using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Security;
using KinkLinkServer.Domain;

namespace KinkLinkServer.Services;

public class PermissionsService
{
    private readonly ILogger<PermissionsService> _logger;
    private readonly PairsService _pairsService;
    private readonly KinkLinkProfilesService _profilesService;

    public PermissionsService(
        Configuration config,
        ILogger<PermissionsService> logger,
        PairsService pairsService,
        KinkLinkProfilesService profilesService
    )
    {
        _logger = logger;
        _pairsService = pairsService;
        _profilesService = profilesService;
    }

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

            var userProfile = await _profilesService.GetIdFromUidAsync(userUID);
            var targetProfile = await _profilesService.GetIdFromUidAsync(targetUID);

            if (userProfile == null)
            {
                _logger.LogWarning(
                    "CreatePairRequest failed: user UID {UserUID} not found",
                    userUID
                );
                return DBPairResult.PairUIDDoesNotExist;
            }

            if (targetProfile == null)
            {
                _logger.LogWarning(
                    "CreatePairRequest failed: target UID {TargetUID} not found",
                    targetUID
                );
                return DBPairResult.PairUIDDoesNotExist;
            }

            var userId = userProfile.Value;
            var targetId = targetProfile.Value;

            var existingPairs = await _pairsService.GetAllPairsForProfileAsync(userUID);

            var hasOneWayPair = existingPairs.Any(p => p.Id == userId && p.PairId == targetId);
            var hasReversePair = existingPairs.Any(p => p.Id == targetId && p.PairId == userId);

            if (hasOneWayPair && hasReversePair)
            {
                _logger.LogInformation(
                    "CreatePairRequest: users already paired {UserUID} <-> {TargetUID}",
                    userUID,
                    targetUID
                );
                return DBPairResult.Paired;
            }

            if (hasOneWayPair)
            {
                _logger.LogInformation(
                    "CreatePairRequest: onesided pair already exists from {UserUID} to {TargetUID}",
                    userUID,
                    targetUID
                );
                return DBPairResult.OnesidedPairExists;
            }

            var newPair = await _pairsService.AddPairAsync(userId, targetId);

            if (newPair != null)
            {
                _logger.LogInformation(
                    "CreatePairRequest: successfully created pair from {UserUID} to {TargetUID}",
                    userUID,
                    targetUID
                );
                return DBPairResult.PairCreated;
            }

            _logger.LogError(
                "CreatePairRequest: failed to create pair from {UserUID} to {TargetUID}",
                userUID,
                targetUID
            );
            return DBPairResult.UnknownError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePairRequest failed with unexpected error");
            return DBPairResult.UnknownError;
        }
    }

    public async Task<DBPairResult> UpdatePermissions(
        string senderFriendCode,
        string targetFriendCode,
        UserPermissions permissions
    )
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(senderFriendCode)
                || string.IsNullOrWhiteSpace(targetFriendCode)
            )
            {
                _logger.LogWarning("UpdatePermissions failed: invalid friend codes provided");
                return DBPairResult.PairUIDDoesNotExist;
            }

            if (senderFriendCode == targetFriendCode)
            {
                _logger.LogWarning("UpdatePermissions failed: cannot update permissions with self");
                return DBPairResult.PairUIDDoesNotExist;
            }

            var senderProfile = await _profilesService.GetIdFromUidAsync(senderFriendCode);
            var targetProfile = await _profilesService.GetIdFromUidAsync(targetFriendCode);

            if (senderProfile == null)
            {
                _logger.LogWarning(
                    "UpdatePermissions failed: sender UID {SenderUID} not found",
                    senderFriendCode
                );
                return DBPairResult.PairUIDDoesNotExist;
            }

            if (targetProfile == null)
            {
                _logger.LogWarning(
                    "UpdatePermissions failed: target UID {TargetUID} not found",
                    targetFriendCode
                );
                return DBPairResult.PairUIDDoesNotExist;
            }

            var senderId = senderProfile.Value;
            var targetId = targetProfile.Value;

            var existingPairs = await _pairsService.GetAllPairsForProfileAsync(senderFriendCode);
            var existingPair = existingPairs.FirstOrDefault(p =>
                p.Id == senderId && p.PairId == targetId
            );

            if (existingPair == default)
            {
                _logger.LogWarning(
                    "UpdatePermissions failed: no existing pair from {SenderUID} to {TargetUID}",
                    senderFriendCode,
                    targetFriendCode
                );
                return DBPairResult.NoOp;
            }

            var updateResult = await _pairsService.UpdatePairPermissionsAsync(
                senderId,
                targetId,
                (int)permissions.Priority,
                (int)permissions.Gags,
                (int)permissions.Wardrobe,
                (int)permissions.Moodles
            );

            if (updateResult != null)
            {
                _logger.LogInformation(
                    "UpdatePermissions: successfully updated permissions from {SenderUID} to {TargetUID}",
                    senderFriendCode,
                    targetFriendCode
                );
                return DBPairResult.Success;
            }

            _logger.LogError(
                "UpdatePermissions: failed to update permissions from {SenderUID} to {TargetUID}",
                senderFriendCode,
                targetFriendCode
            );
            return DBPairResult.UnknownError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdatePermissions failed with unexpected error");
            return DBPairResult.UnknownError;
        }
    }

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

            var userPairs = await _pairsService.GetAllPairsForProfileAsync(userUID);

            if (!userPairs.Any())
            {
                _logger.LogWarning("GetPermissions failed: user UID {UserUID} not found", userUID);
                return null;
            }

            var targetPair = userPairs.FirstOrDefault(p => p.PairUid == targetUID);

            if (targetPair == default)
            {
                _logger.LogInformation(
                    "GetPermissions: no permissions found from {UserUID} to {TargetUID}",
                    userUID,
                    targetUID
                );
                return null;
            }

            var userPermissions = new Pair(
                targetPair.Id,
                targetPair.PairId,
                targetPair.Expires,
                (int)targetPair.Priority,
                targetPair.ControlsPerm,
                targetPair.ControlsConfig,
                targetPair.DisableSafeword,
                (int)targetPair.Gags,
                (int)targetPair.Wardrobe,
                (int)targetPair.Moodles
            );

            _logger.LogDebug(
                "GetPermissions: retrieved permissions from {UserUID} to {TargetUID}",
                userUID,
                targetUID
            );
            return userPermissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPermissions failed with unexpected error");
            return null;
        }
    }

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

            var userProfile = await _profilesService.GetIdFromUidAsync(userUID);
            var targetProfile = await _profilesService.GetIdFromUidAsync(targetUID);

            if (userProfile == null)
            {
                _logger.LogWarning(
                    "DeletePermissions failed: user UID {UserUID} not found",
                    userUID
                );
                return DBPairResult.PairUIDDoesNotExist;
            }

            if (targetProfile == null)
            {
                _logger.LogWarning(
                    "DeletePermissions failed: target UID {TargetUID} not found",
                    targetUID
                );
                return DBPairResult.PairUIDDoesNotExist;
            }

            var userId = userProfile.Value;
            var targetId = targetProfile.Value;

            var existingPairs = await _pairsService.GetAllPairsForProfileAsync(userUID);
            var existingPair = existingPairs.FirstOrDefault(p =>
                p.Id == userId && p.PairId == targetId
            );

            if (existingPair == default)
            {
                _logger.LogWarning(
                    "DeletePermissions failed: no existing pair from {UserUID} to {TargetUID}",
                    userUID,
                    targetUID
                );
                return DBPairResult.NoOp;
            }

            var deleteResult = await _pairsService.RemovePairAsync(userId, targetId);

            if (deleteResult)
            {
                _logger.LogInformation(
                    "DeletePermissions: successfully deleted pair from {UserUID} to {TargetUID}",
                    userUID,
                    targetUID
                );
                return DBPairResult.Success;
            }

            _logger.LogError(
                "DeletePermissions: failed to delete pair from {UserUID} to {TargetUID}",
                userUID,
                targetUID
            );
            return DBPairResult.UnknownError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeletePermissions failed with unexpected error");
            return DBPairResult.UnknownError;
        }
    }

    private async Task<bool> IsTwoWayPaired(string user, string pair)
    {
        var userProfile = await _profilesService.GetIdFromUidAsync(user);
        var pairProfile = await _profilesService.GetIdFromUidAsync(pair);
        if (userProfile == null || pairProfile == null)
            return false;

        return await _pairsService.ConfirmTwoWayPairAsync(userProfile.Value, pairProfile.Value);
    }

    public async Task<List<TwoWayPermissions>> GetAllPermissions(string userUID)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userUID))
            {
                _logger.LogWarning("GetAllPermissions failed: invalid UID provided");
                return new List<TwoWayPermissions>();
            }

            var allPairs = await _pairsService.GetAllPairsForProfileAsync(userUID);
            var result = new List<TwoWayPermissions>();

            foreach (var pair in allPairs)
            {
                var pairModel = new Pair(
                    pair.Id,
                    pair.PairId,
                    pair.Expires,
                    (int)pair.Priority,
                    pair.ControlsPerm,
                    pair.ControlsConfig,
                    pair.DisableSafeword,
                    (int)pair.Gags,
                    (int)pair.Wardrobe,
                    (int)pair.Moodles
                );
                if (pair.PairUid != null)
                {
                    var permission = new TwoWayPermissions(
                        userUID,
                        pair.PairUid,
                        pairModel,
                        new Pair()
                    );
                    result.Add(permission);
                }
            }

            _logger.LogDebug(
                "GetAllPermissions: retrieved {Count} permissions for UID {UserUID}",
                result.Count,
                userUID
            );
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllPermissions failed with unexpected error");
            return new List<TwoWayPermissions>();
        }
    }
}
