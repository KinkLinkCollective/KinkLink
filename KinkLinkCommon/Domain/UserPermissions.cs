using KinkLinkCommon.Database;
using MessagePack;
using static KinkLinkCommon.Database.QueriesSql;

namespace KinkLinkCommon.Domain;

/// <summary>
///     Stores the primary and linkshell permissions that make up the total permissions a user can grant another
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public record UserPermissions
{
    public DateTime? Expires;
    public bool ToggleTimerLocks, TogglePermanentLocks, ToggleGarbler, LockGarbler, ToggleChannels, LockChannels;
    public bool ApplyGag, LockGag, UnlockGag, RemoveGag;
    public bool ApplyWardrobe, LockWardrobe, UnlockWardrobe, RemoveWardrobe;
    public bool ApplyMoodles, LockMoodles, UnlockMoodles, RemoveMoodles;

    public UserPermissions()
    {

    }
    /// <summary>
    ///     <inheritdoc cref="UserPermissions"/>
    /// </summary>
    public UserPermissions(Pair pair)
    {
        Expires = pair.Expires;
        ToggleTimerLocks = pair.ToggleTimerLocks;
        TogglePermanentLocks = pair.TogglePermanentLocks;
        ToggleGarbler = pair.ToggleGarbler;
        LockGarbler = pair.LockGarbler;
        ToggleChannels = pair.ToggleChannels;
        LockChannels = pair.LockChannels;
        ApplyGag = pair.ApplyGag;
        LockGag = pair.LockGag;
        UnlockGag = pair.UnlockGag;
        RemoveGag = pair.RemoveGag;
        ApplyWardrobe = pair.ApplyWardrobe;
        LockWardrobe = pair.LockWardrobe;
        UnlockWardrobe = pair.UnlockWardrobe;
        RemoveWardrobe = pair.RemoveWardrobe;
        ApplyMoodles = pair.ApplyMoodles;
        LockMoodles = pair.LockMoodles;
        UnlockMoodles = pair.UnlockMoodles;
        RemoveMoodles = pair.RemoveMoodles;
    }
}
