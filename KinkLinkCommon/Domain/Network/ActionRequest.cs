using MessagePack;

namespace KinkLinkCommon.Domain.Network;

[MessagePackObject]
public record ActionRequest(
    [property: Key(0)] List<string> TargetFriendCodes
);
