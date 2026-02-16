using KinkLinkCommon.Database;
using KinkLinkCommon.Domain;
using KinkLinkCommon.Domain.Enums;
using KinkLinkServer.Domain;

namespace KinkLinkServer.Services;

public class PairsService
{
    private readonly PairsSql _pairsSql;
    private readonly ProfilesSql _profilesSql;

    public PairsService(Configuration config)
    {
        _pairsSql = new PairsSql(config.DatabaseConnectionString);
        _profilesSql = new ProfilesSql(config.DatabaseConnectionString);
    }

    private async Task<int?> GetProfileIdFromUidAsync(string uid)
    {
        var profile = await _profilesSql.GetProfileByUidAsync(new(uid));
        return profile?.Id;
    }

    public async Task<List<UserPermissions>> GetAllPairsForProfileAsync(string uid)
    {
        var result = await _pairsSql.GetAllPairsForProfileAsync(new(uid));
        return result.Select(row => new UserPermissions(
            row.Id,
            row.PairId,
            row.Expires,
            (RelationshipPriority)(row.Priority ?? 0),
            row.ControlsPerm ?? false,
            row.ControlsConfig ?? false,
            row.DisableSafeword ?? false,
            row.Gags,
            row.Wardrobe,
            row.Moodles
        )
        {
            PairUid = row.PairUid
        }).ToList();
    }

    public async Task<UserPermissions?> GetPairByProfileIdsAsync(string uid, string pairUid)
    {
        var id = await GetProfileIdFromUidAsync(uid);
        var pairId = await GetProfileIdFromUidAsync(pairUid);

        if (id is null || pairId is null)
            return null;

        var result = await _pairsSql.GetPairByProfileIdsAsync(new(id.Value, pairId.Value));
        if (result is not { } row)
            return null;

        return new UserPermissions(
            row.Id,
            row.PairId,
            row.Expires,
            (RelationshipPriority)(row.Priority ?? 0),
            row.ControlsPerm ?? false,
            row.ControlsConfig ?? false,
            row.DisableSafeword ?? false,
            row.Gags,
            row.Wardrobe,
            row.Moodles
        );
    }

    public async Task<UserPermissions?> AddPairAsync(int id, int pairId)
    {
        var result = await _pairsSql.AddPairAsync(new(id, pairId));
        if (result is not { } row)
            return null;

        return new UserPermissions(
            row.Id,
            row.PairId,
            row.Expires,
            (RelationshipPriority)(row.Priority ?? 0),
            row.ControlsPerm ?? false,
            row.ControlsConfig ?? false,
            row.DisableSafeword ?? false,
            row.Gags,
            row.Wardrobe,
            row.Moodles
        );
    }

    public async Task<UserPermissions?> AddTemporaryPairAsync(int id, int pairId, DateTime? expires)
    {
        var result = await _pairsSql.AddTemporaryPairAsync(new(id, pairId, expires));
        if (result is not { } row)
            return null;

        return new UserPermissions(
            row.Id,
            row.PairId,
            row.Expires,
            (RelationshipPriority)(row.Priority ?? 0),
            row.ControlsPerm ?? false,
            row.ControlsConfig ?? false,
            row.DisableSafeword ?? false,
            row.Gags,
            row.Wardrobe,
            row.Moodles
        );
    }

    public async Task<bool> RemovePairAsync(int id, int pairId)
    {
        var result = await _pairsSql.RemovePairAsync(new(id, pairId));
        return result is { };
    }

    public async Task<UserPermissions?> UpdatePairPermissionsAsync(
        int uid,
        int pairUid,
        int priority,
        int gags,
        int wardrobe,
        int moodles
    )
    {
        var result = await _pairsSql.UpdatePairPermissionsAsync(
            new(priority, gags, wardrobe, moodles, uid, pairUid)
        );
        if (result is not { } row)
            return null;

        return new UserPermissions(
            row.Id,
            row.PairId,
            row.Expires,
            (RelationshipPriority)(row.Priority ?? 0),
            row.ControlsPerm ?? false,
            row.ControlsConfig ?? false,
            row.DisableSafeword ?? false,
            row.Gags,
            row.Wardrobe,
            row.Moodles
        );
    }

    public async Task<UserPermissions?> UpdatePairControlPermissionsAsync(
        int uid,
        int pairUid,
        bool controlsPerm,
        bool controlsConfig,
        bool disableSafeword
    )
    {
        var result = await _pairsSql.UpdatePairControlPermissionsAsync(
            new(controlsPerm, controlsConfig, disableSafeword, uid, pairUid)
        );
        if (result is not { } row)
            return null;

        return new UserPermissions(
            row.Id,
            row.PairId,
            row.Expires,
            (RelationshipPriority)(row.Priority ?? 0),
            row.ControlsPerm ?? false,
            row.ControlsConfig ?? false,
            row.DisableSafeword ?? false,
            row.Gags,
            row.Wardrobe,
            row.Moodles
        );
    }

    public async Task<bool> ConfirmTwoWayPairAsync(int id, int pairId)
    {
        var result = await _pairsSql.ConfirmTwoWayPairAsync(new(id, pairId));
        return result?.Twowaypair ?? false;
    }

    public async Task<int> PurgeExpiredPairsAsync()
    {
        var result = await _pairsSql.PurgeExpiredPairsAsync();
        return result != null ? 1 : 0;
    }

    public async Task<bool> HasExpiredPairsAsync()
    {
        var result = await _pairsSql.HasExpiredPairsAsync();
        return result?.HasExpired ?? false;
    }
}
