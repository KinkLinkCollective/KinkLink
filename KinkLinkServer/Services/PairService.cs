using KinkLinkCommon.Database;
using KinkLinkCommon.Domain;
using KinkLinkServer.Domain;

namespace KinkLinkServer.Services;

public class PairsService
{
    private readonly PairsSql _pairsSql;

    public PairsService(Configuration config)
    {
        _pairsSql = new PairsSql(config.DatabaseConnectionString);
    }

    public async Task<List<UserPermissions>> GetAllPairsForProfileAsync(string uid)
    {
        // TODO :This function should return a list of UserPermissions associated with this UID.
        // First lookup the Id, then find all _to way_ pair lookups for this specific profile
        throw new NotImplementedException("Function not implemented");
    }

    public async Task<UserPermissions?> GetPairByProfileIdsAsync(string uid, string pairUid)
    {
        // TODO : Get the latest data for this _two way_ pair
        throw new NotImplementedException("Function not implemented");
    }

    public async Task<bool> AddPairAsync(int id, int pairId)
    {
        // TODO: Create a one way pair request for this user.
        // Return true if successfully created false if not created for any reason.
        throw new NotImplementedException();
    }

    public async Task<UserPermissions?> AddTemporaryPairAsync(int id, int pairId, DateTime? expires)
    {
        // TODO: Create a pair as temporary as temporary.
        // Return true if successfully created, return false if failed
        throw new NotImplementedException();
    }

    public async Task<bool> RemovePairAsync(int id, int pairId)
    {
        // TODO: Implement this function.
        // Get the id for `uid` and `pairUid` then delete the entry for both sides
        throw new NotImplementedException();
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
        // TODO: Implement this function
        // Get the id for `uid` and `pairUid` then update the values in the table
        throw new NotImplementedException();
    }

    public async Task<UserPermissions?> UpdatePairControlPermissionsAsync(
        int uid,
        int pairUid,
        bool controlsPerm,
        bool controlsConfig,
        bool disableSafeword
    )
    {
        // TODO: Implement this function
        // Get the id for `uid` and `pairUid` then update the values in the table
        throw new NotImplementedException();
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
