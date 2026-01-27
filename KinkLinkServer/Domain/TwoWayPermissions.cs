using KinkLinkCommon.Database;

namespace KinkLinkServer.Domain;

/// <summary>
///     A bidirectional set of permissions
/// </summary>
public record TwoWayPermissions
{
    /// <summary>
    ///     The owner of the permissions
    /// </summary>
    public readonly string UserUID;

    /// <summary>
    ///     The target of the permissions
    /// </summary>
    public readonly string TargetUID;

    /// <summary>
    ///     The permissions the owner has granted to the target
    /// </summary>
    public readonly Pair PermissionsGrantedTo;

    /// <summary>
    ///     The permissions the target has granted to the owner
    /// </summary>
    public readonly Pair? PermissionsGrantedBy;

    /// <summary>
    ///     <inheritdoc cref="TwoWayPermissions"/>
    /// </summary>
    public TwoWayPermissions(string friendCode, string targetFriendCode, Pair grantedTo, Pair grantedBy)
    {
        UserUID = friendCode;
        TargetUID = targetFriendCode;
        PermissionsGrantedTo = grantedTo;
        PermissionsGrantedBy = grantedBy;
    }
}
