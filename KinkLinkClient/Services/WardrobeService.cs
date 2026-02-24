using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KinkLinkClient;
using KinkLinkClient.Dependencies.Glamourer.Domain;
using KinkLinkClient.Dependencies.Glamourer.Services;
using KinkLinkClient.Dependencies.Penumbra.Services;
using KinkLinkClient.Domain.Configurations;
using KinkLinkClient.Domain.Dependencies.Glamourer;
using KinkLinkClient.Domain.Dependencies.Glamourer.Components;
using KinkLinkClient.Utils;
using Newtonsoft.Json.Linq;

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

//
public record WardrobeItem
{
    public Guid Id;
    public string Name = string.Empty;
    public string Description = string.Empty;
    public GlamourerEquipmentSlot Slot;
    public GlamourerItem? Item;
    public Dictionary<string, GlamourerMaterial> Materials = [];
}

public class ActiveWardrobe
{
    private GlamourerDesign? BaseLayer = null;
    private readonly Dictionary<GlamourerEquipmentSlot, GlamourerItem?> _equipment = new();
    private GlamourerShow? Hat = new();
    private GlamourerIsToggled? Visor = new();

    public bool IsActive()
    {
        return BaseLayer != null
            || _equipment.Values.Any(v => v != null)
            || Hat != null
            || Visor != null;
    }

    public void SetBaseLayer(GlamourerDesign design)
    {
        BaseLayer = design;
    }

    public void ClearBaseLayer()
    {
        BaseLayer = null;
    }

    public void SetIndividual(GlamourerEquipmentSlot slot, GlamourerItem item)
    {
        _equipment[slot] = item;
    }

    public void ClearIndividual(GlamourerEquipmentSlot slot)
    {
        _equipment.Remove(slot);
    }

    public GlamourerDesign GetCurrentState()
    {
        if (!IsActive())
        {
            Plugin.Log.Error("There is nothing currently set. This should not have been called");
            return new();
        }
        var final = BaseLayer ?? new();

        foreach (var (slot, item) in _equipment)
        {
            if (item is not null)
            {
                SetEquipmentSlot(final.Equipment, slot, item);
                // TODO: Handle the material states
            }
        }

        return final;
    }

    public List<GlamourerMod> GetMods()
    {
        var modlist = new List<GlamourerMod>();
        if (BaseLayer is { } baselayer)
        {
            modlist.AddRange(baselayer.Mods);
        }
        return modlist;
    }

    private static void SetEquipmentSlot(
        GlamourerEquipment equipment,
        GlamourerEquipmentSlot slot,
        GlamourerItem item
    )
    {
        switch (slot)
        {
            case GlamourerEquipmentSlot.Head:
                equipment.Head = item;
                break;
            case GlamourerEquipmentSlot.Body:
                equipment.Body = item;
                break;
            case GlamourerEquipmentSlot.Hands:
                equipment.Hands = item;
                break;
            case GlamourerEquipmentSlot.Legs:
                equipment.Legs = item;
                break;
            case GlamourerEquipmentSlot.Feet:
                equipment.Feet = item;
                break;
            case GlamourerEquipmentSlot.Ears:
                equipment.Ears = item;
                break;
            case GlamourerEquipmentSlot.Neck:
                equipment.Neck = item;
                break;
            case GlamourerEquipmentSlot.Wrists:
                equipment.Wrists = item;
                break;
            case GlamourerEquipmentSlot.RFinger:
                equipment.RFinger = item;
                break;
            case GlamourerEquipmentSlot.LFinger:
                equipment.LFinger = item;
                break;
        }
    }
}

public class WardrobeService : IDisposable
{
    private readonly PenumbraService _penumbraService;
    private readonly GlamourerService _glamourerService;

    private Dictionary<string, WardrobeItem> _wardrobeItems = [];
    private Dictionary<string, GlamourerDesign> _wardrobeSets = [];

    public ActiveWardrobe ActiveSet { get; private set; }

    public IReadOnlyList<WardrobeItem> WardrobePieces => [.. _wardrobeItems.Values];
    public IReadOnlyList<GlamourerDesign> ImportedSets => [.. _wardrobeSets.Values];

    public bool IsGlamourerApiAvailable => _glamourerService.ApiAvailable;

    public WardrobeService(GlamourerService glamourerService, PenumbraService penumbraService)
    {
        _penumbraService = penumbraService;
        _glamourerService = glamourerService;
        ActiveSet = new();

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

    public void AddPiece(WardrobeItem piece)
    {
        _wardrobeItems[piece.Name] = piece;
        _ = SaveAsync();
    }

    public void UpdatePiece(WardrobeItem piece)
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

    public WardrobeItem? GetPieceById(Guid id)
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

    public async Task<List<Design>> RefreshGlamourerDesignsAsync()
    {
        if (
            _glamourerService.ApiAvailable
            && await _glamourerService.GetDesignList().ConfigureAwait(false) is { } designs
        )
            return designs.OrderBy(d => d.Path).ToList();
        else
            return new List<Design>();
    }

    public async Task<GlamourerItem?> GetGlamourSlotFromPlayer(GlamourerEquipmentSlot slot)
    {
        var designJson = await _glamourerService.GetDesignComponentsAsync(
            GlamourerService.PLAYER_ID
        );
        if (designJson is not JObject jObject)
        {
            Plugin.Log.Error("Design JSON is not a valid JObject");
            return null;
        }

        var glamourerDesign = GlamourerDesignHelper.FromJObject(jObject);
        if (glamourerDesign == null)
        {
            Plugin.Log.Error("Failed to convert design JSON to GlamourerDesign");
            return null;
        }

        switch (slot)
        {
            case GlamourerEquipmentSlot.Head:
                return glamourerDesign.Equipment.Head;
            case GlamourerEquipmentSlot.Body:
                return glamourerDesign.Equipment.Body;
            case GlamourerEquipmentSlot.Hands:
                return glamourerDesign.Equipment.Hands;
            case GlamourerEquipmentSlot.Legs:
                return glamourerDesign.Equipment.Legs;
            case GlamourerEquipmentSlot.Feet:
                return glamourerDesign.Equipment.Feet;
            case GlamourerEquipmentSlot.Ears:
                return glamourerDesign.Equipment.Ears;
            case GlamourerEquipmentSlot.Neck:
                return glamourerDesign.Equipment.Neck;
            case GlamourerEquipmentSlot.Wrists:
                return glamourerDesign.Equipment.Wrists;
            case GlamourerEquipmentSlot.RFinger:
                return glamourerDesign.Equipment.RFinger;
            case GlamourerEquipmentSlot.LFinger:
                return glamourerDesign.Equipment.LFinger;
            default:
                return null;
        }
    }

    public async Task<GlamourerDesign?> GetDesignAsync(Guid designId)
    {
        var designJson = await _glamourerService.GetDesignJObjectAsync(designId);
        if (designJson is not JObject jObject)
        {
            Plugin.Log.Error("Design JSON is not a valid JObject");
            return null;
        }

        var glamourerDesign = GlamourerDesignHelper.FromJObject(jObject);
        if (glamourerDesign == null)
        {
            Plugin.Log.Error("Failed to convert design JSON to GlamourerDesign");
        }
        return glamourerDesign;
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

        ActiveSet.SetBaseLayer(set);

        foreach (var modsettings in ActiveSet.GetMods())
        {
            (Mod mod, ModSettings settings) = modsettings.ToTuple();
            await _penumbraService.SetTemporaryModState(mod, settings, true);
        }

        await _glamourerService.ApplyDesignAsync(ActiveSet.GetCurrentState());
    }

    public async Task RemoveActiveSetAsync()
    {
        if (!_glamourerService.ApiAvailable || ActiveSet == null)
        {
            return;
        }

        foreach (var modsettings in ActiveSet.GetMods())
        {
            (Mod mod, ModSettings settings) = modsettings.ToTuple();
            await _penumbraService.SetTemporaryModState(mod, settings, false);
        }
        // Set the active set to null _before_ reverting to prevent it from reapplyign the equipment
        ActiveSet.ClearBaseLayer();

        await _glamourerService.RevertToAutomation();
    }

    public async Task ReapplyIfChanged(GlamourerDesign design)
    {
        // If there's no active set, just don't bother with anything.
        if (!ActiveSet.IsActive())
            return;

        var currentState = ActiveSet.GetCurrentState();
        if (!WardrobeService.EquippedItemsChanged(currentState.Equipment, design.Equipment))
            return;

        await _glamourerService.ApplyDesignAsync(currentState);
    }

    /// <summary>
    ///     Tests to see if any equipment marked with 'apply' are different
    /// </summary>
    private static bool EquippedItemsChanged(
        GlamourerEquipment activeset,
        GlamourerEquipment newState
    )
    {
        // Only check if the permanent transformation affects a certain item
        if (activeset.Head.Apply && activeset.Head.IsEqualTo(newState.Head) is false)
            return true;
        if (activeset.Body.Apply && activeset.Body.IsEqualTo(newState.Body) is false)
            return true;
        if (activeset.Hands.Apply && activeset.Hands.IsEqualTo(newState.Hands) is false)
            return true;
        if (activeset.Legs.Apply && activeset.Legs.IsEqualTo(newState.Legs) is false)
            return true;
        if (activeset.Feet.Apply && activeset.Feet.IsEqualTo(newState.Feet) is false)
            return true;
        if (activeset.Ears.Apply && activeset.Ears.IsEqualTo(newState.Ears) is false)
            return true;
        if (activeset.Neck.Apply && activeset.Neck.IsEqualTo(newState.Neck) is false)
            return true;
        if (activeset.Wrists.Apply && activeset.Wrists.IsEqualTo(newState.Wrists) is false)
            return true;
        if (activeset.RFinger.Apply && activeset.RFinger.IsEqualTo(newState.RFinger) is false)
            return true;
        if (activeset.LFinger.Apply && activeset.LFinger.IsEqualTo(newState.LFinger) is false)
            return true;
        return false;
    }

    public void Dispose()
    {
        _glamourerService.IpcReady -= OnIpcReady;
        // If the wardrobe is disposed all temporary mods should be unlocked
        _penumbraService.ClearAllTemporaryMods();
        GC.SuppressFinalize(this);
    }
}
