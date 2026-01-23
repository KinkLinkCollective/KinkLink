using MessagePack;

namespace KinkLinkCommon.Domain.Network.RemoveFriend;

[MessagePackObject]
public record RemoveFriendRequest(
    [property: Key(0)] string TargetFriendCode
);
