using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KinkLinkClient.Dependencies.Glamourer.Domain;
using KinkLinkClient.Dependencies.Glamourer.Services;
using KinkLinkClient.Domain;
using KinkLinkClient.Domain.Dependencies.Glamourer;
using KinkLinkClient.Domain.Dependencies.Glamourer.Components;
using KinkLinkClient.Managers;
using KinkLinkClient.Services;
using KinkLinkClient.Utils;
using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Domain.Network;

namespace KinkLinkClient.UI.Views.Wardrobe;

public enum SubView
{
    List,
    Import,
    Editor,
}

public enum ListTab
{
    Items,
    Sets,
}

// TODO: This class needs to be implemented
public class WardrobeViewUiController : IDisposable
{
    // Injected
    private readonly GlamourerService _glamourerService;
    private readonly NetworkService _networkService;
    private readonly SelectionManager _selectionManager;

    // View State
    public SubView CurrentView = SubView.List;
    public ListTab CurrentTab = ListTab.Items;

    // Wardrobe Data (To display)
    public List<RestraintItem> WardrobePieces = [];
    public List<GlamourerDesign> ImportedSets = [];

    // Selection State
    public Guid? SelectedPieceId;
    public Guid? SelectedSetId;

    // Editor State
    public RestraintItem? EditingPiece;
    public GlamourerDesign? EditingSet;

    // Editor Fields
    public string EditedName = string.Empty;
    public string EditedDescription = string.Empty;

    // Slot Editor State
    public string SelectedSlotName = "Head";
    public GlamourerItem EditedItem = new();
    public uint EditedDye1;
    public uint EditedDye2;

    public static string[] AllSlotNames =>
        ["Head", "Body", "Hands", "Legs", "Feet", "Ears", "Neck", "Wrists", "RFinger", "LFinger"];

    public static string GetSlotDisplayName(string slotName)
    {
        return slotName switch
        {
            "Head" => "Head",
            "Body" => "Body",
            "Hands" => "Hands",
            "Legs" => "Legs",
            "Feet" => "Feet",
            "Ears" => "Earrings",
            "Neck" => "Necklace",
            "Wrists" => "Bracelet",
            "RFinger" => "Right Ring",
            "LFinger" => "Left Ring",
            _ => slotName,
        };
    }

    public static GlamourerEquipmentSlot GetSlotFromName(string slotName)
    {
        return slotName switch
        {
            "Head" => GlamourerEquipmentSlot.Head,
            "Body" => GlamourerEquipmentSlot.Body,
            "Hands" => GlamourerEquipmentSlot.Hands,
            "Legs" => GlamourerEquipmentSlot.Legs,
            "Feet" => GlamourerEquipmentSlot.Feet,
            "Ears" => GlamourerEquipmentSlot.Ears,
            "Neck" => GlamourerEquipmentSlot.Neck,
            "Wrists" => GlamourerEquipmentSlot.Wrists,
            "RFinger" => GlamourerEquipmentSlot.RFinger,
            "LFinger" => GlamourerEquipmentSlot.LFinger,
            _ => GlamourerEquipmentSlot.None,
        };
    }

    public void SaveSlotData()
    {
        if (EditingPiece == null)
            return;

        var slot = GetSlotFromName(SelectedSlotName);
        EditingPiece.Name = EditedName;
        EditingPiece.Description = EditedDescription;
        EditingPiece.Slot = slot;
        EditingPiece.Item = EditedItem.Clone();
        EditingPiece.Dye1 = EditedDye1 > 0 ? EditedDye1 : null;
        EditingPiece.Dye2 = EditedDye2 > 0 ? EditedDye2 : null;
    }

    public void LoadSlotData()
    {
        if (EditingPiece == null)
            return;

        EditedName = EditingPiece.Name;
        EditedDescription = EditingPiece.Description;
        SelectedSlotName = EditingPiece.Slot.ToString();
        EditedItem = EditingPiece.Item.Clone();
        EditedDye1 = EditingPiece.Dye1 ?? 0;
        EditedDye2 = EditingPiece.Dye2 ?? 0;
    }

    public void LoadSetData()
    {
        if (EditingSet == null)
            return;

        EditedName = EditingSet.Name;
        EditedDescription = EditingSet.Description;
    }

    public void ResetEditorFields()
    {
        EditedName = string.Empty;
        EditedDescription = string.Empty;
        SelectedSlotName = "Head";
        EditedItem = new GlamourerItem();
        EditedDye1 = 0;
        EditedDye2 = 0;
    }

    public void SaveSetData()
    {
        if (EditingSet == null)
            return;

        EditingSet.Name = EditedName;
        EditingSet.Description = EditedDescription;
    }

    public RestraintItem? GetSelectedPiece() =>
        SelectedPieceId.HasValue
            ? WardrobePieces.FirstOrDefault(p => p.Id == SelectedPieceId)
            : null;

    public GlamourerDesign? GetSelectedSet() =>
        SelectedSetId.HasValue
            ? ImportedSets.FirstOrDefault(s => s.Identifier == SelectedSetId)
            : null;

    public void OpenEditor(RestraintItem? piece = null)
    {
        EditingPiece =
            piece
            ?? new RestraintItem
            {
                Id = Guid.NewGuid(),
                Name = "New Piece",
                Description = string.Empty,
                Slot = GlamourerEquipmentSlot.Head,
                Item = new GlamourerItem
                {
                    Apply = true,
                    ApplyCrest = false,
                    ApplyStain = true,
                    Crest = false,
                    ItemId = 0,
                    Stain = 0,
                    Stain2 = 0,
                },
            };
        EditingSet = null;
        LoadSlotData();
        CurrentView = SubView.Editor;
    }

    public void OpenSetEditor(GlamourerDesign? set = null)
    {
        EditingSet = set;
        EditingPiece = null;
        if (set != null)
            LoadSetData();
        CurrentView = SubView.Editor;
    }

    public void CloseEditor()
    {
        ResetEditorFields();
        EditingPiece = null;
        EditingSet = null;
        CurrentView = SubView.List;
    }

    public async Task SaveEditorAsync()
    {
        if (EditingPiece != null)
        {
            SaveSlotData();

            var existing = WardrobePieces.FirstOrDefault(p => p.Id == EditingPiece.Id);
            if (existing != null)
            {
                var index = WardrobePieces.IndexOf(existing);
                WardrobePieces[index] = EditingPiece;
            }
            else
            {
                WardrobePieces.Add(EditingPiece);
            }
        }
        else if (EditingSet != null)
        {
            SaveSetData();

            var existing = ImportedSets.FirstOrDefault(s => s.Identifier == EditingSet.Identifier);
            if (existing != null)
            {
                var index = ImportedSets.IndexOf(existing);
                ImportedSets[index] = EditingSet;
            }
        }

        if (Plugin.CharacterConfiguration is { } config)
        {
            if (EditingPiece != null)
            {
                config.WardrobeItems[EditingPiece.Name] = EditingPiece;
            }
            else if (EditingSet != null)
            {
                config.WardrobeSets[EditingSet.Name] = EditingSet.Clone();
            }

            await config.Save();
        }

        CloseEditor();
    }

    public void DeletePiece(Guid id)
    {
        var piece = WardrobePieces.FirstOrDefault(p => p.Id == id);
        if (piece != null)
        {
            WardrobePieces.Remove(piece);
            if (SelectedPieceId == id)
                SelectedPieceId = null;
        }
    }

    public void ImportDesign(
        GlamourerDesign design,
        string? name = null,
        string? description = null
    )
    {
        design.Name = name ?? design.Name;
        design.Description = description ?? string.Empty;
        ImportedSets.Add(design);
        SelectedSetId = design.Identifier;
    }

    public async Task ImportDesignByIdAsync(
        Guid designId,
        string? name = null,
        string? description = null
    )
    {
        var designJson = await _glamourerService
            .GetDesignJObjectAsync(designId)
            .ConfigureAwait(false);
        Plugin.Log.Info($"Design Json {designJson}");

        if (designJson is not { } jObject)
        {
            Plugin.Log.Error($"{designJson} is not a valid JObject");
            return;
        }

        var glamourerDesign = GlamourerDesignHelper.FromJObject(jObject);
        if (glamourerDesign == null)
        {
            Plugin.Log.Error($"{glamourerDesign} is not a valid design");
            return;
        }

        ImportDesign(glamourerDesign, name, description);
    }

    public void DeleteSet(Guid id)
    {
        var set = ImportedSets.FirstOrDefault(s => s.Identifier == id);
        if (set != null)
        {
            ImportedSets.Remove(set);
            if (SelectedSetId == id)
                SelectedSetId = null;
        }
    }

    /// <summary>
    ///     Search for the design we'd like to send
    /// </summary>
    public string SearchTerm = string.Empty;

    /// <summary>
    ///     The currently selected Guid of the Design to send
    /// </summary>
    public Guid SelectedDesignId = Guid.Empty;

    /// <summary>
    ///     Cached list of designs
    /// </summary>
    private List<Design>? _sorted;

    /// <summary>
    ///     Filtered cached list of designs
    /// </summary>
    private List<Design>? _filtered;

    /// <summary>
    ///     The designs to display in the Ui
    /// </summary>
    public List<Design>? Designs => SearchTerm == string.Empty ? _sorted : _filtered;

    public bool ShouldApplyCustomization = true;
    public bool ShouldApplyEquipment = true;

    /// <summary>
    ///     <inheritdoc cref="TransformationViewUiController"/>
    /// </summary>
    public WardrobeViewUiController(
        GlamourerService glamourer,
        NetworkService networkService,
        SelectionManager selectionManager
    )
    {
        _glamourerService = glamourer;
        _networkService = networkService;
        _selectionManager = selectionManager;

        LoadFromConfiguration();

        _glamourerService.IpcReady += OnIpcReady;
        if (_glamourerService.ApiAvailable)
            _ = RefreshGlamourerDesigns();
    }

    public void LoadFromConfiguration()
    {
        if (Plugin.CharacterConfiguration is not { } config)
            return;

        WardrobePieces = config.WardrobeItems.Values.ToList();
        ImportedSets = config.WardrobeSets.Values.ToList();
    }

    /// <summary>
    ///     Filters the sorted design list by search term
    /// </summary>
    public void FilterDesignsBySearchTerm()
    {
        _filtered = _sorted is not null ? FilterDesigns(_sorted, SearchTerm).ToList() : null;
    }

    /// <summary>
    ///     Recursive method to filter nodes based on both folders and content names
    /// </summar
    private List<Design> FilterDesigns(IEnumerable<Design> designs, string searchTerms)
    {
        // Reset the selected so possibly unselected designs aren't stored
        SelectedDesignId = Guid.Empty;

        var filtered = new List<Design>();
        foreach (var design in designs)
        {
            if (design.Path.Contains(searchTerms, StringComparison.OrdinalIgnoreCase))
            {
                filtered.Add(design);
            }
        }

        return filtered;
    }

    public async Task RefreshGlamourerDesigns()
    {
        SelectedDesignId = Guid.Empty;

        if (await _glamourerService.GetDesignList().ConfigureAwait(false) is not { } designs)
            return;

        // Assignment
        _sorted = designs.OrderBy(key => key.Path).ToList();
    }

    // public async Task SendDesign()
    // {
    //     if (SelectedDesignId == Guid.Empty)
    //         return;
    //
    //     var flags = GlamourerApplyFlags.Once;
    //     if (ShouldApplyCustomization)
    //         flags |= GlamourerApplyFlags.Customization;
    //     if (ShouldApplyEquipment)
    //         flags |= GlamourerApplyFlags.Equipment;
    //
    //     // Don't send one with nothing
    //     if (flags is GlamourerApplyFlags.Once)
    //         return;
    //
    //     if (
    //         await _glamourerService.GetDesignAsync(SelectedDesignId).ConfigureAwait(false)
    //         is not { } design
    //     )
    //         return;
    //
    //     await _networkCommandManager
    //         .SendTransformation(_selectionManager.GetSelectedFriendCodes(), design, flags)
    //         .ConfigureAwait(false);
    // }
    //
    /// <summary>
    ///     The dictionary returned by glamourer is not sorted, so we will recursively go through and sort the children
    /// </summary>
    private static void SortTree<T>(FolderNode<T> root)
    {
        // Copy all the children from this node and sort them by folder, then name
        var sorted = root
            .Children.Values.OrderByDescending(node => node.IsFolder)
            .ThenBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Clear all the children with the values sorted and copied
        root.Children.Clear();

        // Reintroduce because dictionaries preserve insertion order
        foreach (var node in sorted)
            root.Children[node.Name] = node;

        // Recursively sort the remaining children
        foreach (var child in root.Children.Values)
            SortTree(child);
    }

    private void OnIpcReady(object? sender, EventArgs e)
    {
        _ = RefreshGlamourerDesigns();
    }

    public void Dispose()
    {
        _glamourerService.IpcReady -= OnIpcReady;
        GC.SuppressFinalize(this);
    }
}
