using MessagePack;

namespace KinkLinkCommon.Domain.Network.UpdateFriend;

[MessagePackObject(true)]
public record UpdateFriendResponse(
    [property: Key(0)] UpdateFriendEc Result
);
