using System;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using KinkLinkClient.Domain;
using KinkLinkClient.Domain.Dependencies.Glamourer;
using KinkLinkClient.Services;
using KinkLinkClient.Utils;

namespace KinkLinkClient.UI.Views.Wardrobe;

public partial class WardrobeViewUi
{
    private const int DetailsPaneWidth = 250;
    private const int ListTabHeight = 40;

    private void DrawListView()
    {
        var padding = ImGui.GetStyle().WindowPadding;
        var windowWidth = ImGui.GetWindowWidth() - padding.X * 2;
        var contentWidth = windowWidth - DetailsPaneWidth - padding.X;

        SharedUserInterfaces.ContentBox(
            "WardrobeListTabs",
            KinkLinkStyle.PanelBackground,
            true,
            () =>
            {
                var tabButtonWidth = (contentWidth - padding.X * 4 - 40);

                if (controller.CurrentTab == ListTab.Items)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, KinkLinkStyle.PrimaryColor);
                    if (ImGui.Button("Items", new Vector2(tabButtonWidth, ListTabHeight)))
                    {
                        controller.CurrentTab = ListTab.Items;
                    }
                    ImGui.PopStyleColor();
                }
                else
                {
                    if (ImGui.Button("Items", new Vector2(tabButtonWidth, ListTabHeight)))
                    {
                        controller.CurrentTab = ListTab.Items;
                    }
                }

                ImGui.SameLine(tabButtonWidth + padding.X * 2);

                if (controller.CurrentTab == ListTab.Sets)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, KinkLinkStyle.PrimaryColor);
                    if (ImGui.Button("Sets", new Vector2(tabButtonWidth, ListTabHeight)))
                    {
                        controller.CurrentTab = ListTab.Sets;
                    }
                    ImGui.PopStyleColor();
                }
                else
                {
                    if (ImGui.Button("Sets", new Vector2(tabButtonWidth, ListTabHeight)))
                    {
                        controller.CurrentTab = ListTab.Sets;
                    }
                }

                ImGui.SameLine(windowWidth - padding.X * 2 - 40);

                var addButtonAction =
                    controller.CurrentTab == ListTab.Items
                        ? (Action)(() => controller.OpenEditor())
                        : () => controller.CurrentView = SubView.Import;

                if (
                    SharedUserInterfaces.IconButton(
                        FontAwesomeIcon.Plus,
                        new Vector2(ListTabHeight, ListTabHeight),
                        controller.CurrentTab == ListTab.Items
                            ? "Add New Piece"
                            : "Switch to Import"
                    )
                )
                    addButtonAction();

                SharedUserInterfaces.Tooltip(
                    controller.CurrentTab == ListTab.Items
                        ? "Add New Piece"
                        : "Switch to Import to add sets"
                );
            }
        );

        var headerHeight = ImGui.GetItemRectSize().Y + padding.Y;
        var listHeight = (ImGui.GetWindowHeight() - headerHeight - padding.Y * 3) * 0.5f;

        ImGui.BeginChild(
            "##WardrobeListPane",
            new Vector2(contentWidth, listHeight),
            true,
            ImGuiWindowFlags.AlwaysVerticalScrollbar
        );

        switch (controller.CurrentTab)
        {
            case ListTab.Items:
                DrawItemsList();
                break;
            case ListTab.Sets:
                DrawSetsList();
                break;
        }

        ImGui.EndChild();

        ImGui.SameLine();

        DrawDetailsPane(listHeight);

        DrawActiveSlotsPanel();
    }

    private void DrawActiveSlotsPanel()
    {
        var padding = ImGui.GetStyle().WindowPadding;
        var windowWidth = ImGui.GetWindowWidth() - padding.X * 2;
        var contentWidth = windowWidth - DetailsPaneWidth - padding.X;

        ImGui.Spacing();

        SharedUserInterfaces.ContentBox(
            "ActiveSlotsPanel",
            KinkLinkStyle.PanelBackground,
            true,
            () =>
            {
                SharedUserInterfaces.MediumText("Active Slots");
                ImGui.Separator();

                var slotStatuses = controller.GetActiveSlotStatuses();
                var buttonWidth = 70f;
                var labelWidth = contentWidth - buttonWidth - padding.X * 2;

                foreach (var status in slotStatuses)
                {
                    var slotDisplayName =
                        status.SlotName == "BaseSet"
                            ? "BaseSet"
                            : WardrobeViewUiController.GetSlotDisplayName(status.SlotName);

                    ImGui.SetCursorPosX(padding.X);

                    if (status.HasItem)
                    {
                        ImGui.TextColored(
                            new Vector4(0, 1, 0, 1),
                            $"{slotDisplayName}: {status.ItemDisplay}"
                        );
                    }
                    else
                    {
                        ImGui.TextColored(ImGuiColors.DalamudGrey, $"{slotDisplayName}: Empty");
                    }

                    ImGui.SameLine(contentWidth - buttonWidth - padding.X);

                    var buttonLabel = status.HasItem
                        ? "Remove##" + status.SlotName
                        : "##Empty" + status.SlotName;
                    if (ImGui.Button(buttonLabel, new Vector2(buttonWidth, 24)))
                    {
                        _ = RemoveSlotWithErrorHandling(status.SlotName);
                    }

                    ImGui.Spacing();
                }
            }
        );
    }

    private async Task RemoveSlotWithErrorHandling(string slotName)
    {
        try
        {
            await controller.RemoveSlotItemAsync(slotName);
            NotificationHelper.Success("Remove Slot", $"Removed from {slotName}");
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "Failed to remove slot item");
            NotificationHelper.Error("Remove Slot", "Failed to remove. Check logs for details.");
        }
    }

    private void DrawItemsList()
    {
        foreach (var piece in wardrobeService.WardrobePieces)
        {
            DrawSelectableItem(
                piece.Name,
                piece.Id.ToString(),
                controller.SelectedPieceId == piece.Id,
                () =>
                {
                    controller.SelectedPieceId = piece.Id;
                    controller.SelectedSetId = null;
                },
                $"Slot: {piece.Slot}"
            );
        }

        if (wardrobeService.WardrobePieces.Count == 0)
        {
            ImGui.TextColored(ImGuiColors.DalamudGrey, "No wardrobe pieces yet.");
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Click + to add one.");
        }
    }

    private void DrawSetsList()
    {
        foreach (var set in wardrobeService.ImportedSets)
        {
            DrawSelectableItem(
                set.Name,
                set.Identifier.ToString(),
                controller.SelectedSetId == set.Identifier,
                () =>
                {
                    controller.SelectedSetId = set.Identifier;
                    controller.SelectedPieceId = null;
                },
                null
            );
        }

        if (wardrobeService.ImportedSets.Count == 0)
        {
            ImGui.TextColored(ImGuiColors.DalamudGrey, "No imported sets yet.");
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Click + to switch to Import.");
        }
    }

    private void DrawSelectableItem(
        string name,
        string id,
        bool isSelected,
        Action onSelect,
        string? tooltip
    )
    {
        var label = $"{name}##{id}";

        if (isSelected)
        {
            ImGui.PushStyleColor(ImGuiCol.Header, KinkLinkStyle.PrimaryColor);
            if (ImGui.Selectable(label, true))
            {
                onSelect();
            }
            ImGui.PopStyleColor();
        }
        else
        {
            if (ImGui.Selectable(label))
            {
                onSelect();
            }
        }

        if (tooltip != null && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }
    }

    private void DrawDetailsPane(float height)
    {
        var padding = ImGui.GetStyle().WindowPadding;

        if (ImGui.BeginChild("##WardrobeDetailsPane", new Vector2(DetailsPaneWidth, height), false))
        {
            SharedUserInterfaces.ContentBox(
                "WardrobeDetails",
                KinkLinkStyle.PanelBackground,
                true,
                () => SharedUserInterfaces.MediumText("Details")
            );

            var selectedPiece = controller.GetSelectedPiece();
            var selectedSet = controller.GetSelectedSet();

            if (selectedPiece != null)
            {
                DrawPieceDetails(selectedPiece);
            }
            else if (selectedSet != null)
            {
                DrawSetDetails(selectedSet);
            }
            else
            {
                ImGui.TextColored(
                    ImGuiColors.DalamudGrey,
                    "Select an item or set to view details."
                );
            }

            ImGui.EndChild();
        }
    }

    private void DrawPieceDetails(WardrobeItem piece)
    {
        ImGui.Spacing();
        SharedUserInterfaces.MediumText(piece.Name);
        ImGui.Separator();

        if (!string.IsNullOrEmpty(piece.Description))
        {
            ImGui.TextWrapped(piece.Description);
            ImGui.Spacing();
        }

        ImGui.Text($"Slot: {piece.Slot}");
        ImGui.Text($"Item ID: {piece.Item?.ItemId ?? 0}");
        ImGui.Text($"Apply: {(piece.Item?.Apply == true ? "Yes" : "No")}");

        ImGui.Spacing();

        var buttonWidth =
            (ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X * 4) / 3
            - ImGui.GetStyle().ItemSpacing.X;

        if (ImGui.Button("Apply", new Vector2(buttonWidth, 30)))
        {
            _ = ApplyPieceWithErrorHandling(piece);
        }

        ImGui.SameLine(buttonWidth + ImGui.GetStyle().ItemSpacing.X);

        if (ImGui.Button("Edit", new Vector2(buttonWidth, 30)))
        {
            controller.OpenEditor(piece);
        }

        ImGui.SameLine((buttonWidth + ImGui.GetStyle().ItemSpacing.X) * 2);

        if (ImGui.Button("Delete", new Vector2(buttonWidth, 30)))
        {
            controller.DeletePiece(piece.Id);
        }
    }

    private async Task ApplyPieceWithErrorHandling(WardrobeItem piece)
    {
        try
        {
            await controller.ApplyPieceAsync(piece);
            NotificationHelper.Success("Apply Piece", $"Applied {piece.Name}");
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "Failed to apply piece");
            NotificationHelper.Error(
                "Apply Piece",
                "Failed to apply piece. Check logs for details."
            );
        }
    }

    private void DrawSetDetails(GlamourerDesign set)
    {
        ImGui.Spacing();
        SharedUserInterfaces.MediumText(set.Name);
        ImGui.Separator();

        if (!string.IsNullOrEmpty(set.Description))
        {
            ImGui.TextWrapped(set.Description);
            ImGui.Spacing();
        }

        ImGui.Text($"Design: {set.Name}");

        ImGui.Spacing();

        var buttonWidth =
            (ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X * 4) / 3
            - ImGui.GetStyle().ItemSpacing.X;

        if (ImGui.Button("Apply", new Vector2(buttonWidth, 30)))
        {
            _ = ApplySetWithErrorHandling(set.Name);
        }

        ImGui.SameLine(buttonWidth + ImGui.GetStyle().ItemSpacing.X);

        if (ImGui.Button("Edit", new Vector2(buttonWidth, 30)))
        {
            controller.OpenSetEditor(set);
        }

        ImGui.SameLine((buttonWidth + ImGui.GetStyle().ItemSpacing.X) * 2);

        if (ImGui.Button("Delete", new Vector2(buttonWidth, 30)))
        {
            controller.DeleteSet(set.Identifier);
        }
    }

    private async Task ApplySetWithErrorHandling(string name)
    {
        try
        {
            await controller.ApplySetAsync(name);
            NotificationHelper.Success("Apply Set", $"Applied set: {name}");
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "Failed to apply set");
            NotificationHelper.Error("Apply Set", "Failed to apply set. Check logs for details.");
        }
    }
}
