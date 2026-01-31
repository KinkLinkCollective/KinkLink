using System;
using KinkLinkClient.Handlers.Network.Base;
using KinkLinkClient.Services;
using KinkLinkClient.UI.Views.Chat;
using KinkLinkCommon.Domain.Network;
using Microsoft.AspNetCore.SignalR.Client;

namespace KinkLinkClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="GlobalMessageCommand"/>
/// </summary>
public class ChatMessageReceivedHandler : AbstractNetworkHandler, IDisposable
{
    // Instantiated
    private readonly IDisposable _handler;
    private readonly ChatViewUiController _controller;
    /// <summary>
    ///     <inheritdoc cref="ChatMessageReceivedHandler"/>
    /// </summary>
    public ChatMessageReceivedHandler(ChatViewUiController controller, LogService log, FriendsListService friendsList, NetworkService network, PauseService pause) : base(friendsList, log, pause)
    {
        _controller = controller;
        _handler = network.Connection.On<ChatReceivedMessage>(HubMethod.ReceiveChatMessage, Handle);
    }

    /// <summary>
    ///     Handles incoming global chat messages
    /// </summary>
    private void Handle(ChatReceivedMessage command)
    {
        Plugin.Log.Verbose($"Chat message from {command.Alias} ({command.Timestamp}): {command.Message}");

        // Add message to chat UI
        _controller.AddMessage(command);
    }

    /// <summary>
    ///     Handles incoming global users list updates
    /// </summary>
    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}
