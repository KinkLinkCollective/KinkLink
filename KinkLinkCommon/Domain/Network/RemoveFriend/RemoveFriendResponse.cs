using MessagePack;

namespace KinkLinkCommon.Domain.Network.RemoveFriend;

[MessagePackObject]
public record RemoveFriendResponse(
    [property: Key(0)] RemoveFriendEc Result
);
