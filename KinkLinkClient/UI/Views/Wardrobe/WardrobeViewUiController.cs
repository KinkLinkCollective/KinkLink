using System;
using System.Collections.Generic;
using KinkLinkClient.Services;
using KinkLinkClient.Utils;
using KinkLinkCommon.Domain.Network;

namespace KinkLinkClient.UI.Views.Wardrobe;

// TODO: This class needs to be implemented
public class WardrobeViewUiController : IDisposable
{
    public bool IsBusy => _busy;
    public string InputMessage = string.Empty;
    public bool ScrollToBottom = true;

    // Injected
    private readonly NetworkService _network;
    private readonly WorldService _world;

    private bool _busy = false;

    public WardrobeViewUiController(NetworkService network, WorldService world)
    {
        _network = network;
        _world = world;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}