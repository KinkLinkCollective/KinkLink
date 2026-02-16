using KinkLinkCommon.Database;
using KinkLinkCommon.Domain.Enums;
using MessagePack;

namespace KinkLinkCommon.Domain;

/// <summary>
///     Stores the primary and linkshell permissions that make up the total permissions a user can grant another
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public record UserPermissions
{
    public int Id;
    public int PairId;
    public string? PairUid;
    public DateTime? Expires;
    public RelationshipPriority Priority;
    public bool ControlsPerm;
    public bool ControlsConfig;
    public bool DisableSafeword;
    public GagPermissions Gags;
    public WardrobePermissions Wardrobe;
    public MoodlesPermissions Moodles;

    public UserPermissions() { }

    /// <summary>
    ///     <inheritdoc cref="UserPermissions"/>
    /// </summary>
    public UserPermissions(Pair pair)
    {
        // TODO: Reimplement when the new bitmasks are enabled
        // Convert the filetype for pair into a user permission
    }

    public UserPermissions(
        bool temporary,
        RelationshipPriority priority,
        GagPermissions gags,
        WardrobePermissions wardrobe,
        MoodlesPermissions moodles
    )
    {
        if (temporary)
        {
            Expires = DateTime.UtcNow.AddDays(1);
        }
        Gags = gags;
        Wardrobe = wardrobe;
        Moodles = moodles;
    }

    public UserPermissions(
        DateTime? expires,
        RelationshipPriority priority,
        bool controlsPerm,
        bool controlsConfig,
        bool disableSafeword,
        int? gags,
        int? wardrobe,
        int? moodles
    )
    {
        Expires = expires;
        Priority = priority;
        ControlsPerm = controlsPerm;
        ControlsConfig = controlsConfig;
        DisableSafeword = disableSafeword;
        Gags = (GagPermissions)(gags ?? 0);
        Wardrobe = (WardrobePermissions)(wardrobe ?? 0);
        Moodles = (MoodlesPermissions)(moodles ?? 0);
    }

    public UserPermissions(
        DateTime? expires,
        RelationshipPriority priority,
        bool controlsPerm,
        bool controlsConfig,
        bool disableSafeword,
        int? gags,
        int? wardrobe,
        int? moodles,
        string? profileUid,
        string? pairUid
    )
    {
        Expires = expires;
        Priority = priority;
        ControlsPerm = controlsPerm;
        ControlsConfig = controlsConfig;
        DisableSafeword = disableSafeword;
        Gags = (GagPermissions)(gags ?? 0);
        Wardrobe = (WardrobePermissions)(wardrobe ?? 0);
        Moodles = (MoodlesPermissions)(moodles ?? 0);
        PairUid = pairUid;
    }

    public UserPermissions(
        int id,
        int pairId,
        DateTime? expires,
        RelationshipPriority priority,
        bool controlsPerm,
        bool controlsConfig,
        bool disableSafeword,
        int? gags,
        int? wardrobe,
        int? moodles
    )
    {
        Id = id;
        PairId = pairId;
        Expires = expires;
        Priority = priority;
        ControlsPerm = controlsPerm;
        ControlsConfig = controlsConfig;
        DisableSafeword = disableSafeword;
        Gags = (GagPermissions)(gags ?? 0);
        Wardrobe = (WardrobePermissions)(wardrobe ?? 0);
        Moodles = (MoodlesPermissions)(moodles ?? 0);
    }
}
