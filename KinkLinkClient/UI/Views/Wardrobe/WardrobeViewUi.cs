using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using KinkLinkClient.Dependencies.Glamourer.Domain;
using KinkLinkClient.Domain;
using KinkLinkClient.Domain.Interfaces;
using KinkLinkClient.Utils;

namespace KinkLinkClient.UI.Views.Wardrobe;

// TODO: This class needs to be implemented
public class WardrobeViewUi(WardrobeViewUiController controller) : IDrawable
{
    // Const
    private const int ActionButtonHeight = 40;

    private const int SendDesignButtonHeight = 40;

    public void Draw()
    {
        ImGui.BeginChild("WardrobeContent", Vector2.Zero, false, KinkLinkStyle.ContentFlags);

        var width = ImGui.GetWindowWidth();
        var padding = ImGui.GetStyle().WindowPadding;

        var begin = ImGui.GetCursorPosY();

        Header();
        DesignSearch(width, padding);

        var headerHeight = ImGui.GetCursorPosY() - begin;
        var designContextBoxSize = new Vector2(
            0,
            ImGui.GetWindowHeight() - headerHeight - padding.X * 6 - SendDesignButtonHeight * 2
        );
        if (controller.Designs is { } designs)
            DesignDisplay(designs, designContextBoxSize);
        ImGui.Spacing();
        DesignOptions();
        SharedUserInterfaces.ContentBox(
            "DesignAdd",
            KinkLinkStyle.PanelBackground,
            false,
            () =>
            {
                if (
                    ImGui.Button(
                        "Transform",
                        new Vector2(ImGui.GetWindowWidth() - padding.X * 2, SendDesignButtonHeight)
                    )
                )
                    NotificationHelper.Success("Success", "TODO: Apply Glamour");
            }
        );

        ImGui.EndChild();
        ImGui.SameLine();
    }

    private void DesignDisplay(IEnumerable<Design> nodes, Vector2 designContextBoxSize)
    {
        if (
            ImGui.BeginChild(
                "##DesignsDisplayBox",
                designContextBoxSize,
                true,
                ImGuiWindowFlags.AlwaysVerticalScrollbar
            )
        )
        {
            if (controller.Designs is { } designs)
            {
                foreach (var design in nodes)
                {
                    if (controller.SelectedDesignId == design.Id)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Header, KinkLinkStyle.PrimaryColor);
                        ImGui.Selectable(design.Path, true);
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        if (ImGui.Selectable($"{design.Path}"))
                        {
                            controller.SelectedDesignId = design.Id;
                        }
                    }
                }
            }
            ImGui.EndChild();
        }
    }

    private void Header()
    {
        SharedUserInterfaces.ContentBox(
            "Wardrobe",
            KinkLinkStyle.PanelBackground,
            true,
            () =>
            {
                SharedUserInterfaces.BigTextCentered("Wardrobe");
            }
        );
    }

    private void DesignSearch(float width, Vector2 padding)
    {
        SharedUserInterfaces.ContentBox(
            "GlamourerDesignSearch",
            KinkLinkStyle.PanelBackground,
            true,
            () =>
            {
                SharedUserInterfaces.MediumText("Select Design");

                ImGui.SetNextItemWidth(width - padding.X * 4 - ImGui.GetFontSize());
                if (
                    ImGui.InputTextWithHint(
                        "##DesignSearchBar",
                        "Search",
                        ref controller.SearchTerm,
                        32
                    )
                )
                    controller.FilterDesignsBySearchTerm();

                ImGui.SameLine();

                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Designs"))
                    _ = controller.RefreshGlamourerDesigns();
            }
        );
    }

    private void DesignOptions()
    {
        SharedUserInterfaces.ContentBox(
            "DesignOptions",
            KinkLinkStyle.PanelBackground,
            true,
            () =>
            {
                if (controller.ShouldApplyCustomization)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, KinkLinkStyle.PrimaryColor);
                    if (
                        SharedUserInterfaces.IconButton(
                            FontAwesomeIcon.User,
                            new Vector2(SendDesignButtonHeight)
                        )
                    )
                        controller.ShouldApplyCustomization = !controller.ShouldApplyCustomization;
                    ImGui.PopStyleColor();
                }
                else
                {
                    if (
                        SharedUserInterfaces.IconButton(
                            FontAwesomeIcon.User,
                            new Vector2(SendDesignButtonHeight)
                        )
                    )
                        controller.ShouldApplyCustomization = !controller.ShouldApplyCustomization;
                }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(
                        controller.ShouldApplyCustomization
                            ? "Currently applying customizations, click to disable"
                            : "Not applying customizations, click to enable"
                    );

                ImGui.SameLine();

                if (controller.ShouldApplyEquipment)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, KinkLinkStyle.PrimaryColor);
                    if (
                        SharedUserInterfaces.IconButton(
                            FontAwesomeIcon.Tshirt,
                            new Vector2(SendDesignButtonHeight)
                        )
                    )
                        controller.ShouldApplyEquipment = !controller.ShouldApplyEquipment;
                    ImGui.PopStyleColor();
                }
                else
                {
                    if (
                        SharedUserInterfaces.IconButton(
                            FontAwesomeIcon.Tshirt,
                            new Vector2(SendDesignButtonHeight)
                        )
                    )
                        controller.ShouldApplyEquipment = !controller.ShouldApplyEquipment;
                }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(
                        controller.ShouldApplyEquipment
                            ? "Currently applying equipment, click to disable"
                            : "Not applying equipment, click to enable"
                    );
            }
        );
    }
}
