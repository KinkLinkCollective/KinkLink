using System;
using System.Linq;
using System.Threading.Tasks;
using KinkLinkClient.Domain.Dependencies.Glamourer;
using KinkLinkClient.Domain.Dependencies.Glamourer.Components;
using KinkLinkClient.Services;
using KinkLinkCommon.Domain.Enums;

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

public class WardrobeViewUiController
{
    private readonly WardrobeService _wardrobeService;

    public WardrobeService WardrobeService => _wardrobeService;

    public SubView CurrentView { get; set; } = SubView.List;
    public ListTab CurrentTab { get; set; } = ListTab.Items;

    public Guid? SelectedPieceId { get; set; }
    public Guid? SelectedSetId { get; set; }

    public RestraintItem? EditingPiece { get; set; }
    public GlamourerDesign? EditingSet { get; set; }

    public string EditedName { get; set; } = string.Empty;
    public string EditedDescription { get; set; } = string.Empty;

    public string SelectedSlotName { get; set; } = "Head";
    public GlamourerItem EditedItem { get; set; } = new();
    public uint EditedDye1 { get; set; }
    public uint EditedDye2 { get; set; }

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

    public WardrobeViewUiController(WardrobeService wardrobeService)
    {
        _wardrobeService = wardrobeService;
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
        EditedItem = EditingPiece.Item?.Clone() ?? new GlamourerItem();
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
            ? _wardrobeService.GetPieceById(SelectedPieceId.Value)
            : null;

    public GlamourerDesign? GetSelectedSet() =>
        SelectedSetId.HasValue
            ? _wardrobeService.GetSetById(SelectedSetId.Value)
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
            _wardrobeService.AddPiece(EditingPiece);
        }
        else if (EditingSet != null)
        {
            SaveSetData();
            _wardrobeService.UpdateSet(EditingSet);
        }

        CloseEditor();
    }

    public void DeletePiece(Guid id)
    {
        _wardrobeService.DeletePiece(id);
        if (SelectedPieceId == id)
            SelectedPieceId = null;
    }

    public void DeleteSet(Guid id)
    {
        _wardrobeService.DeleteSet(id);
        if (SelectedSetId == id)
            SelectedSetId = null;
    }

    public async Task ApplySetAsync(string name)
    {
        await _wardrobeService.ApplySetAsync(name);
    }

    public async Task RemoveActiveSetAsync()
    {
        await _wardrobeService.RemoveActiveSetAsync();
    }

    public bool IsGlamourerApiAvailable => _wardrobeService.IsGlamourerApiAvailable;

    public void FilterDesigns()
    {
        _wardrobeService.FilterDesigns();
    }

    public void RefreshDesigns()
    {
        _ = _wardrobeService.RefreshGlamourerDesignsAsync();
    }
}
