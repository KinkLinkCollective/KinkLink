using System;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using KinkLinkClient.Domain;
using KinkLinkClient.Utils;

namespace KinkLinkClient.UI.Views.Wardrobe;

public partial class WardrobeViewUi
{
    private void DrawEditorView()
    {
        if (controller.EditingPiece is null && controller.EditingSet is null)
        {
            controller.CloseEditor();
            return;
        }

        var padding = ImGui.GetStyle().WindowPadding;
        var width = ImGui.GetWindowWidth() - padding.X * 2;
        var contentWidth = width - padding.X * 2;

        if (controller.EditingPiece is not null)
        {
            SharedUserInterfaces.ContentBox(
                "EditorName",
                KinkLinkStyle.PanelBackground,
                true,
                () =>
                {
                    SharedUserInterfaces.MediumText("Name");
                    ImGui.SetNextItemWidth(contentWidth);
                    var name = controller.EditedName;
                    if (ImGui.InputText("##Name", ref name, 64))
                        controller.EditedName = name;
                }
            );

            SharedUserInterfaces.ContentBox(
                "EditorDescription",
                KinkLinkStyle.PanelBackground,
                true,
                () =>
                {
                    SharedUserInterfaces.MediumText("Description");
                    ImGui.SetNextItemWidth(contentWidth);
                    var description = controller.EditedDescription;
                    if (ImGui.InputText("##Description", ref description, 256))
                        controller.EditedDescription = description;
                }
            );

            SharedUserInterfaces.ContentBox(
                "EditorSlot",
                KinkLinkStyle.PanelBackground,
                true,
                () =>
                {
                    SharedUserInterfaces.MediumText("Slot");
                    ImGui.SetNextItemWidth(contentWidth);
                    if (ImGui.BeginCombo("##SlotSelector", WardrobeViewUiController.GetSlotDisplayName(controller.SelectedSlotName)))
                    {
                        foreach (var slotName in WardrobeViewUiController.AllSlotNames)
                        {
                            if (ImGui.Selectable(slotName))
                            {
                                controller.SelectedSlotName = slotName;
                            }
                        }
                        ImGui.EndCombo();
                    }
                }
            );

            SharedUserInterfaces.ContentBox(
                "EditorItemId",
                KinkLinkStyle.PanelBackground,
                true,
                () =>
                {
                    SharedUserInterfaces.MediumText("Item ID");
                    ImGui.SetNextItemWidth(contentWidth);
                    var itemIdStr = controller.EditedItem.ItemId.ToString();
                    if (ImGui.InputText("##ItemId", ref itemIdStr, 20))
                    {
                        if (string.IsNullOrWhiteSpace(itemIdStr))
                        {
                            controller.EditedItem.ItemId = 0;
                        }
                        else if (uint.TryParse(itemIdStr, out var result))
                        {
                            controller.EditedItem.ItemId = result;
                        }
                    }
                }
            );

            SharedUserInterfaces.ContentBox(
                "EditorDyes",
                KinkLinkStyle.PanelBackground,
                true,
                () =>
                {
                    SharedUserInterfaces.MediumText("Dyes");

                    var dye1Str = controller.EditedDye1 > 0 ? controller.EditedDye1.ToString() : string.Empty;
                    var dye2Str = controller.EditedDye2 > 0 ? controller.EditedDye2.ToString() : string.Empty;

                    ImGui.SetNextItemWidth(contentWidth * 0.5f - padding.X);
                    if (ImGui.InputText("##Dye1", ref dye1Str, 10))
                    {
                        if (string.IsNullOrWhiteSpace(dye1Str))
                        {
                            controller.EditedDye1 = 0;
                        }
                        else if (uint.TryParse(dye1Str, out var result))
                        {
                            controller.EditedDye1 = result;
                        }
                    }

                    ImGui.SameLine(contentWidth * 0.5f);
                    ImGui.SetNextItemWidth(contentWidth * 0.5f - padding.X);
                    if (ImGui.InputText("##Dye2", ref dye2Str, 10))
                    {
                        if (string.IsNullOrWhiteSpace(dye2Str))
                        {
                            controller.EditedDye2 = 0;
                        }
                        else if (uint.TryParse(dye2Str, out var result))
                        {
                            controller.EditedDye2 = result;
                        }
                    }
                }
            );

            SharedUserInterfaces.ContentBox(
                "EditorProperties",
                KinkLinkStyle.PanelBackground,
                true,
                () =>
                {
                    SharedUserInterfaces.MediumText("Properties");

                    var apply = controller.EditedItem.Apply;
                    if (ImGui.Checkbox("Apply Item", ref apply))
                        controller.EditedItem.Apply = apply;

                    var applyStain = controller.EditedItem.ApplyStain;
                    if (ImGui.Checkbox("Apply Stain", ref applyStain))
                        controller.EditedItem.ApplyStain = applyStain;

                    var applyCrest = controller.EditedItem.ApplyCrest;
                    if (ImGui.Checkbox("Apply Crest", ref applyCrest))
                        controller.EditedItem.ApplyCrest = applyCrest;

                    var crest = controller.EditedItem.Crest;
                    if (ImGui.Checkbox("Crest", ref crest))
                        controller.EditedItem.Crest = crest;
                }
            );
        }
        else if (controller.EditingSet != null)
        {
            SharedUserInterfaces.ContentBox(
                "EditorName",
                KinkLinkStyle.PanelBackground,
                true,
                () =>
                {
                    SharedUserInterfaces.MediumText("Name");
                    ImGui.SetNextItemWidth(contentWidth);
                    var name = controller.EditedName;
                    if (ImGui.InputText("##Name", ref name, 64))
                        controller.EditedName = name;
                }
            );

            SharedUserInterfaces.ContentBox(
                "EditorDescription",
                KinkLinkStyle.PanelBackground,
                true,
                () =>
                {
                    SharedUserInterfaces.MediumText("Description");
                    ImGui.SetNextItemWidth(contentWidth);
                    var description = controller.EditedDescription;
                    if (ImGui.InputText("##Description", ref description, 256))
                        controller.EditedDescription = description;
                }
            );

            SharedUserInterfaces.ContentBox(
                "EditorDesignInfo",
                KinkLinkStyle.PanelBackground,
                true,
                () =>
                {
                    SharedUserInterfaces.MediumText("Design Info");
                    ImGui.Text($"Design: {controller.EditingSet.Name}");
                }
            );
        }

        SharedUserInterfaces.ContentBox(
            "EditorSave",
            KinkLinkStyle.PanelBackground,
            false,
            () =>
            {
                var buttonWidth = (contentWidth - padding.X) / 2;

                if (ImGui.Button("Cancel", new Vector2(buttonWidth, 40)))
                {
                    controller.CloseEditor();
                }

                ImGui.SameLine();

                if (ImGui.Button("Save", new Vector2(buttonWidth, 40)))
                {
                    _ = SaveAndHandleErrors();
                }
            }
        );
    }

    private async Task SaveAndHandleErrors()
    {
        try
        {
            await controller.SaveEditorAsync();
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "Failed to save editor changes");
            NotificationHelper.Error("Save Failed", "Unable to save changes. Check logs for details.");
        }
    }
}
