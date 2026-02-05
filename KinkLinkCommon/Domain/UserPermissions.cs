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
    public DateTime? Expires;
    public PermissionsModes Mode;
    public RelationshipPriority Priority;
    public GagPermissions Gags;
    public WardrobePermissions Wardrobe;
    public MoodlesPermissions Moodles;

    public UserPermissions()
    {

    }
    /// <summary>
    ///     <inheritdoc cref="UserPermissions"/>
    /// </summary>
    public UserPermissions(Pair pair)
    {
        // Reimplement when the new bitmasks are enabled
    }
    public UserPermissions(bool temporary, PermissionsModes modes, RelationshipPriority priority, GagPermissions gags, WardrobePermissions wardrobe, MoodlesPermissions moodles) {
        if (temporary) {
            // Set Expires to datetime + 1 day or something defined in a configuration file somewhere.
        }
        Mode = modes;
        Gags = gags;
        Wardrobe = wardrobe;
        Moodles = moodles;
    }
}
