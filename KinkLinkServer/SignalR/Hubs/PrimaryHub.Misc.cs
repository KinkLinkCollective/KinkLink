using KinkLinkCommon.Domain.Network;
using KinkLinkCommon.Domain.Network.Customize;
using KinkLinkCommon.Domain.Network.Honorific;
using KinkLinkCommon.Domain.Network.Moodles;
using Microsoft.AspNetCore.SignalR;

namespace KinkLinkServer.SignalR.Hubs;

public partial class PrimaryHub
{
    [HubMethodName(HubMethod.CustomizePlus)]
    public async Task<ActionResponse> CustomizePlus(CustomizeRequest request)
    {
        return await customizePlusHandler.Handle(FriendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Honorific)]
    public async Task<ActionResponse> Honorific(HonorificRequest request)
    {
        var friendCode = FriendCode;
        LogWithBehavior($"[HonorificRequest] Sender = {friendCode}, Targets = {string.Join(", ", request.TargetFriendCodes)}, Honorific = {request.Honorific}", LogMode.Console);
        return await honorificHandler.Handle(friendCode, request, Clients);
    }

    [HubMethodName(HubMethod.Moodles)]
    public async Task<ActionResponse> GetMoodlesAction(MoodlesRequest request)
    {
        return await moodlesHandler.Handle(FriendCode, request, Clients);
    }
}
