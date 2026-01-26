using KinkLinkCommon.Domain.Enums.Permissions;
using MessagePack;
using static KinkLinkCommon.Database.QueriesSql;

namespace KinkLinkCommon.Domain;

/// <summary>
///     Stores the primary and linkshell permissions that make up the total permissions a user can grant another
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public record UserPermissions
{
    public UpdatePairPermissionsArgs Permissions;
    /// <summary>
    ///     <inheritdoc cref="UserPermissions"/>
    /// </summary>
    public UserPermissions()
    {
    }
}
