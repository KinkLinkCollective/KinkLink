using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Database;
using MessagePack;

namespace KinkLinkCommon.Domain.Network.SyncOnlineStatus;

[MessagePackObject]
public record SyncOnlineStatusCommand(
    string SenderFriendCode,
    [property: Key(1)] FriendOnlineStatus Status,
    [property: Key(2)] Pair? Permissions
) : ActionCommand(SenderFriendCode);
