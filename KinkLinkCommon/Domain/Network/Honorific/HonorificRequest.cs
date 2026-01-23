using KinkLinkCommon.Dependencies.Honorific.Domain;
using MessagePack;

namespace KinkLinkCommon.Domain.Network.Honorific;

[MessagePackObject]
public record HonorificRequest(
    List<string> TargetFriendCodes,
    [property: Key(1)] HonorificInfo Honorific
) : ActionRequest(TargetFriendCodes);
