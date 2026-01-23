using MessagePack;

namespace KinkLinkCommon.Domain.Network.UpdateFriend;

[MessagePackObject]
public record UpdateFriendRequest(
    [property: Key(0)] string TargetFriendCode,
    [property: Key(1)] UserPermissions Permissions
);
