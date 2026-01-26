using MessagePack;

namespace KinkLinkCommon.Domain.Network.RemoveFriend;

[MessagePackObject]
public record RemovePair(
    [property: Key(0)] RemovePairEc Result
);
