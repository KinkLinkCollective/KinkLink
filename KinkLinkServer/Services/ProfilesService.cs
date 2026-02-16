using KinkLinkCommon.Database;
using KinkLinkCommon.Domain;
using KinkLinkServer.Domain;

namespace KinkLinkServer.Services;

public class KinkLinkProfilesService
{
    private readonly ProfilesSql _profilesSql;

    public KinkLinkProfilesService(Configuration config)
    {
        _profilesSql = new ProfilesSql(config.DatabaseConnectionString);
    }

    public async Task<bool> ExistsAsync(string uid)
    {
        var result = await _profilesSql.ProfileExistsAsync(new(uid));
        return result is { } row && row.Exists;
    }

    public async Task<int?> GetIdFromUidAsync(string uid)
    {
        // TODO :This function should return a list of UserPermissions associated with this UID.
        // Lookup and associate the UID with the profile and return the primary key if it exists
        throw new NotImplementedException("Function not implemented");
    }

    public async Task<KinkLinkProfile?> GetProfileByUidAsync(string uid)
    {
        // TODO :This function should return a list of UserPermissions associated with this UID.
        // Lookup and associate the UID with the profile and return the profile if it exists.
        throw new NotImplementedException("Function not implemented");
    }

    public async Task<KinkLinkProfile?> UpdateDetailsByUidAsync(
        string uid,
        // The current title they wish to use.
        string title,
        string alias,
        string chatRole,
        string description
    )
    {
        // TODO: Update the user profile associated with `uid`
        // First lookup the UID to get the profile ID, then update the entry for that UID for the profile
        throw new NotImplementedException("Function not implemented");
    }
}
