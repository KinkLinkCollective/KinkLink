using KinkLinkCommon.Domain.Enums;
using MessagePack;

namespace KinkLinkCommon.Domain.Network.AddFriend;

[MessagePackObject]
public record AddFriendResponse(
    [property: Key(0)] AddFriendEc Result,
    [property: Key(1)] FriendOnlineStatus Status
);
