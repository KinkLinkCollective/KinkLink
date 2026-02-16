namespace KinkLinkCommon.Domain.Enums.Permissions;

// A general list of all possible actions that can be emitted to a user which use the permissions features.
// This should be populated with every action so that the permissions service can approve or deny the request based on the user's perimssions level.
public enum PairAction
{
    // Gag Actions
    ApplyGag,
    LockGag,
    UnlockGag,
    RemoveGag,
    EnableGarbler,
    LockGarbler,
    SetGarblerChannels,
    LockGarblerChannels,

    // Wardrobe
    ApplyWardrobe,
    LockWardrobe,
    UnlockWardrobe,
    RemoveWardrobe,

    // Moodles (TDB when moodles IPC gets update)
    ApplyOwnMoodle,
    ApplyPairsMoodle,
    LockMoodle,
    UnlockMoodle,
    RemoveMoodle,

    // Configuration Actions
    UpdateConfiguration,
}
