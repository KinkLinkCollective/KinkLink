using KinkLinkCommon.Domain.Enums;
using MessagePack;

namespace KinkLinkCommon.Domain.Network;

[MessagePackObject]
public record ActionResponse(
    [property: Key(0)] ActionResponseEc Result,
    [property: Key(1)] Dictionary<string, ActionResultEc> Results
);
