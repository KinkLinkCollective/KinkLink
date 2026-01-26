using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Domain.Network;
using KinkLinkCommon.Domain.Network.SyncPermissions;
using KinkLinkCommon.Domain.Network.UpdateFriend;
using KinkLinkServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace KinkLinkServer.SignalR.Handlers;

public class UpdateFriendHandler(IPresenceService presenceService, IDatabaseService database, ILogger<UpdateFriendHandler> logger)
{
    public async Task<UpdateFriendResponse> Handle(string friendCode, UpdateFriendRequest request, IHubCallerClients clients)
    {
        var databaseResult = await database.UpdatePermissions(friendCode, request.TargetFriendCode, request.Permissions);
        var result = databaseResult switch
        {
            DBPairResult.Success => UpdateFriendEc.Success,
            DBPairResult.NoOp => UpdateFriendEc.NoOp,
            _ => UpdateFriendEc.Unknown
        };

        if (presenceService.TryGet(request.TargetFriendCode) is not { } connectedClient)
            return new UpdateFriendResponse(result);

        try
        {
            var sync = new SyncPermissionsCommand(friendCode, request.Permissions);
            await clients.Client(connectedClient.ConnectionId).SendAsync(HubMethod.SyncPermissions, sync);
        }
        catch (Exception e)
        {
            logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", friendCode, request.TargetFriendCode, e.Message);
        }

        return new UpdateFriendResponse(result);
    }
}
