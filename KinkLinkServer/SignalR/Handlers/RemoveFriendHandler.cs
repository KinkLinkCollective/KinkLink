using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Domain.Network;
using KinkLinkCommon.Domain.Network.RemoveFriend;
using KinkLinkCommon.Domain.Network.SyncOnlineStatus;
using KinkLinkServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using KinkLinkServer.Services;

namespace KinkLinkServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="RemoveFriendRequest"/>
/// </summary>
public class RemoveFriendHandler(IPresenceService presenceService, DatabaseService databaseService, ILogger<RemoveFriendHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<RemovePair> Handle(string senderFriendCode, RemoveFriendRequest request, IHubCallerClients clients)
    {
        var result = await databaseService.DeletePermissions(senderFriendCode, request.TargetFriendCode) switch
        {
            DBPairResult.NoOp => RemovePairEc.NotFriends,
            DBPairResult.Success => RemovePairEc.Success,
            _ => RemovePairEc.Unknown
        };

        // If the request wasn't meaningful
        if (result is not RemovePairEc.Success)
            return new RemovePair(result);

        // If the target isn't online
        if (presenceService.TryGet(request.TargetFriendCode) is not { } friend)
            return new RemovePair(result);

        // If the target is online, but they don't have us added
        if (await databaseService.GetPermissions(request.TargetFriendCode, senderFriendCode) is null)
            return new RemovePair(result);

        try
        {
            // Send a message to say our status goes from online to pending
            var forward = new SyncOnlineStatusCommand(senderFriendCode, FriendOnlineStatus.Pending, null);
            await clients.Client(friend.ConnectionId).SendAsync(HubMethod.SyncOnlineStatus, forward);
        }
        catch (Exception e)
        {
            logger.LogError("Syncing online status {Sender} -> {Target} failed, {Error}", senderFriendCode, request.TargetFriendCode, e);
        }

        // Return always
        return new RemovePair(result);
    }
}
