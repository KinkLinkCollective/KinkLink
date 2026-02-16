using KinkLinkCommon.Database;
using KinkLinkCommon.Domain;
using KinkLinkServer.Domain;

namespace KinkLinkServer.Services;

public class KinkLinkProfileConfigService
{
    private readonly ProfileConfigSql _profileConfigSql;
    private readonly ProfilesSql _profilesSql;

    public KinkLinkProfileConfigService(Configuration config)
    {
        _profileConfigSql = new ProfileConfigSql(config.DatabaseConnectionString);
        _profilesSql = new ProfilesSql(config.DatabaseConnectionString);
    }

    private async Task<int?> GetProfileIdFromUidAsync(string uid)
    {
        var profile = await _profilesSql.GetProfileByUidAsync(new(uid));
        return profile?.Id;
    }

    public async Task<KinkLinkProfileConfig?> GetProfileConfigByUidAsync(string uid)
    {
        var result = await _profileConfigSql.GetProfileConfigByUidAsync(new(uid));
        if (result is not { } row)
            return null;

        return new KinkLinkProfileConfig(
            row.EnableGlamours ?? false,
            row.EnableGarbler ?? false,
            row.EnableGarblerChannels ?? false,
            row.EnableMoodles ?? false
        );
    }

    public async Task<KinkLinkProfileConfig?> UpdateProfileConfigAsync(
        string uid,
        bool enableGlamours,
        bool enableGarbler,
        bool enableGarblerChannels,
        bool enableMoodles
    )
    {
        var profileId = await GetProfileIdFromUidAsync(uid);
        if (profileId is not { } id)
            return null;

        var result = await _profileConfigSql.UpdateProfileConfigAsync(
            new(id, enableGlamours, enableGarbler, enableGarblerChannels, enableMoodles)
        );

        if (result is not { } row)
            return null;

        return new KinkLinkProfileConfig(
            row.EnableGlamours ?? false,
            row.EnableGarbler ?? false,
            row.EnableGarblerChannels ?? false,
            row.EnableMoodles ?? false
        );
    }

    public async Task<KinkLinkProfileConfig?> DeleteProfileConfigByUidAsync(string uid)
    {
        var profileId = await GetProfileIdFromUidAsync(uid);
        if (profileId is not { } id)
            return null;

        var result = await _profileConfigSql.DeleteProfileConfigAsync(new(id));

        if (result is not { } row)
            return null;

        return new KinkLinkProfileConfig(
            row.EnableGlamours ?? false,
            row.EnableGarbler ?? false,
            row.EnableGarblerChannels ?? false,
            row.EnableMoodles ?? false
        );
    }
}
