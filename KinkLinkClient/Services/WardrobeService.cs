using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using KinkLinkClient;
using KinkLinkClient.Dependencies.Glamourer.Domain;
using KinkLinkClient.Dependencies.Glamourer.Services;
using KinkLinkClient.Dependencies.Penumbra.Services;
using KinkLinkClient.Domain.Configurations;
using KinkLinkClient.Domain.Dependencies.Glamourer;
using KinkLinkClient.Domain.Dependencies.Glamourer.Components;

public enum LayerLocks
{
    InnerGag,
    Gag,
    OuterGag,
    Blindfold,
    Collar,
    Head,
    Chest,
    Hands,
    Feet,
    Legs,
    Earring,
    Necklace,
    Wrists,
    RingR,
    RingL,
    Bonus,
    SetLayer,
}

public class RestraintItem
{
    public Guid Id;
    public string Name = string.Empty;
    public string Description = string.Empty;
    public GlamourerEquipmentSlot Slot;
    public GlamourerItem? Item;
    public uint? Dye1;
    public uint? Dye2;
    public Dictionary<string, GlamourerMaterial> Materials = [];
}

public class WardrobeService
{
    private readonly PenumbraService _penumbraService;
    private readonly GlamourerService _glamourerService;

    private Dictionary<string, RestraintItem> _wardrobeItems = [];
    private Dictionary<string, GlamourerDesign> _wardrobeSets = [];

    public GlamourerDesign? ActiveSet { get; private set; }

    public IReadOnlyList<RestraintItem> WardrobePieces => [.. _wardrobeItems.Values];
    public IReadOnlyList<GlamourerDesign> ImportedSets => [.. _wardrobeSets.Values];

    public List<Design>? GlamourerDesigns { get; private set; }
    private List<Design>? _filteredGlamourerDesigns;

    public string GlamourerSearchTerm { get; set; } = string.Empty;
    public Guid SelectedGlamourerDesignId { get; set; } = Guid.Empty;

    public List<Design>? FilteredGlamourerDesigns =>
        string.IsNullOrEmpty(GlamourerSearchTerm) ? GlamourerDesigns : _filteredGlamourerDesigns;

    public bool IsGlamourerApiAvailable => _glamourerService.ApiAvailable;

    public WardrobeService(GlamourerService glamourerService, PenumbraService penumbraService)
    {
        _penumbraService = penumbraService;
        _glamourerService = glamourerService;

        LoadFromConfiguration();

        _glamourerService.IpcReady += OnIpcReady;
        if (_glamourerService.ApiAvailable)
        {
            _ = RefreshGlamourerDesignsAsync();
        }
    }

    private void OnIpcReady(object? sender, EventArgs e)
    {
        _ = RefreshGlamourerDesignsAsync();
    }

    public void LoadFromConfiguration()
    {
        if (Plugin.CharacterConfiguration is { } config)
        {
            _wardrobeItems = config.WardrobeItems ?? [];
            _wardrobeSets = config.WardrobeSets ?? [];
        }
        else
        {
            _wardrobeItems = [];
            _wardrobeSets = [];
        }
    }

    public void AddPiece(RestraintItem piece)
    {
        _wardrobeItems[piece.Name] = piece;
        _ = SaveAsync();
    }

    public void UpdatePiece(RestraintItem piece)
    {
        _wardrobeItems[piece.Name] = piece;
        _ = SaveAsync();
    }

    public void DeletePiece(Guid id)
    {
        var piece = _wardrobeItems.Values.FirstOrDefault(p => p.Id == id);
        if (piece != null)
        {
            _wardrobeItems.Remove(piece.Name);
            _ = SaveAsync();
        }
    }

    public RestraintItem? GetPieceById(Guid id)
    {
        return _wardrobeItems.Values.FirstOrDefault(p => p.Id == id);
    }

    public void AddSet(GlamourerDesign set)
    {
        _wardrobeSets[set.Name] = set;
        _ = SaveAsync();
    }

    public void UpdateSet(GlamourerDesign set)
    {
        _wardrobeSets[set.Name] = set;
        _ = SaveAsync();
    }

    public void DeleteSet(Guid id)
    {
        var set = _wardrobeSets.Values.FirstOrDefault(s => s.Identifier == id);
        if (set != null)
        {
            _wardrobeSets.Remove(set.Name);
            _ = SaveAsync();
        }
    }

    public GlamourerDesign? GetSetById(Guid id)
    {
        return _wardrobeSets.Values.FirstOrDefault(s => s.Identifier == id);
    }

    public GlamourerDesign? GetSetByName(string name)
    {
        return _wardrobeSets.TryGetValue(name, out var set) ? set : null;
    }

    private async Task SaveAsync()
    {
        if (Plugin.CharacterConfiguration is { } config)
        {
            config.WardrobeSets = _wardrobeSets;
            config.WardrobeItems = _wardrobeItems;
            await config.Save();
        }
    }

    public async Task RefreshGlamourerDesignsAsync()
    {
        SelectedGlamourerDesignId = Guid.Empty;

        if (await _glamourerService.GetDesignList().ConfigureAwait(false) is not { } designs)
            return;

        GlamourerDesigns = designs.OrderBy(d => d.Path).ToList();
    }

    public void RefreshDesigns()
    {
        _ = RefreshGlamourerDesignsAsync();
    }

    public async Task<JToken?> GetDesignJObjectAsync(Guid designId)
    {
        return await _glamourerService.GetDesignJObjectAsync(designId);
    }

    public void FilterDesigns()
    {
        if (GlamourerDesigns == null)
        {
            _filteredGlamourerDesigns = null;
            return;
        }

        if (string.IsNullOrEmpty(GlamourerSearchTerm))
        {
            _filteredGlamourerDesigns = null;
            return;
        }

        _filteredGlamourerDesigns = [.. GlamourerDesigns
            .Where(d => d.Path.Contains(GlamourerSearchTerm, StringComparison.OrdinalIgnoreCase))];
    }

    public async Task ApplySetAsync(string name)
    {
        if (!_glamourerService.ApiAvailable)
        {
            Plugin.Log.Warning("Cannot apply set: Glamourer API not available");
            return;
        }

        if (!_wardrobeSets.TryGetValue(name, out var set))
        {
            Plugin.Log.Warning($"Cannot apply set: Set '{name}' not found in wardrobe");
            return;
        }

        ActiveSet = set;

        foreach (var modsettings in set.Mods)
        {
            (Mod mod, ModSettings settings) = modsettings.ToTuple();
            await _penumbraService.SetTemporaryModState(mod, settings, true);
        }

        await _glamourerService.ApplyDesignAsync(set);
    }

    public async Task RemoveActiveSetAsync()
    {
        if (!_glamourerService.ApiAvailable || ActiveSet == null)
        {
            ActiveSet = null;
            return;
        }

        foreach (var modsettings in ActiveSet.Mods)
        {
            (Mod mod, ModSettings settings) = modsettings.ToTuple();
            await _penumbraService.SetTemporaryModState(mod, settings, true);
        }

        await _glamourerService.RevertToAutomation();
        ActiveSet = null;
    }

    public void Dispose()
    {
        _glamourerService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}
