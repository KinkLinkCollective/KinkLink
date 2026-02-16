using KinkLinkCommon.Database;
using KinkLinkCommon.Domain;
using KinkLinkServer.Domain;

namespace KinkLinkServer.Services;

public class KinkLinkProfileConfigService
{
    private readonly ProfileConfigSql _profileConfigSql;

    public KinkLinkProfileConfigService(Configuration config)
    {
        _profileConfigSql = new ProfileConfigSql(config.DatabaseConnectionString);
    }

    public async Task<KinkLinkProfileConfig?> GetProfileConfigByUidAsync(string uid)
    {
        var result = await _profileConfigSql.GetProfileConfigByUidAsync(new(uid));
        // TODO complete this function
        throw new NotImplementedException();
    }

    public async Task<KinkLinkProfileConfig?> UpdateProfileConfigAsync(
        string uid,
        bool enableGlamours,
        bool enableGarbler,
        bool enableGarblerChannels,
        bool enableMoodles
    )
    {
        // TODO complete this functionm.
        // Get the first profile, update the values then update it
        throw new NotImplementedException();
    }

    public async Task<KinkLinkProfileConfig?> DeleteProfileConfigByUidAsync(string uid)
    {
        // TODO complete this function
        // First convert the UID to an Id and then do the query
        throw new NotImplementedException();
    }
}
