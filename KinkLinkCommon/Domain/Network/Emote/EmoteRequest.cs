using MessagePack;

namespace KinkLinkCommon.Domain.Network.Emote;

[MessagePackObject]
public record EmoteRequest(
    List<string> TargetFriendCodes,
    [property: Key(1)] string Emote,
    [property: Key(2)] bool DisplayLogMessage
) : ActionRequest(TargetFriendCodes);
