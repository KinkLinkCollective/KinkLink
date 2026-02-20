using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using KinkLinkClient.Dependencies.Glamourer.Domain;
using KinkLinkClient.Utils;

namespace KinkLinkClient.UI.Views.Wardrobe;

public partial class WardrobeViewUi
{
    private void DrawImportView()
    {
        var padding = ImGui.GetStyle().WindowPadding;
        var width = ImGui.GetWindowWidth() - padding.X * 2;

        SharedUserInterfaces.ContentBox(
            "ImportDesignSearch",
            KinkLinkStyle.PanelBackground,
            true,
            () =>
            {
                SharedUserInterfaces.MediumText("Search Design to Import");

                ImGui.SetNextItemWidth(width - padding.X * 4 - ImGui.GetFontSize());
                if (ImGui.InputTextWithHint("##ImportSearchBar", "Search", ref controller.SearchTerm, 32))
                    controller.FilterDesignsBySearchTerm();

                ImGui.SameLine();

                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Sync, null, "Refresh Designs"))
                    _ = controller.RefreshGlamourerDesigns();
            }
        );

        SharedUserInterfaces.ContentBox(
            "ImportName",
            KinkLinkStyle.PanelBackground,
            true,
            () =>
            {
                SharedUserInterfaces.MediumText("Name");
                ImGui.SetNextItemWidth(width - padding.X * 2);
                ImGui.InputText("##ImportName", ref controller.EditedName, 64);
            }
        );

        SharedUserInterfaces.ContentBox(
            "ImportDescription",
            KinkLinkStyle.PanelBackground,
            true,
            () =>
            {
                SharedUserInterfaces.MediumText("Description");
                ImGui.SetNextItemWidth(width - padding.X * 2);
                ImGui.InputText("##ImportDescription", ref controller.EditedDescription, 256);
            }
        );

        var windowHeight = ImGui.GetWindowHeight();
        var designBoxHeight = (windowHeight - padding.Y * 16 - ImportButtonHeight - 140) * 0.5f;

        SharedUserInterfaces.ContentBox(
            "ImportDesignList",
            KinkLinkStyle.PanelBackground,
            true,
            () =>
            {
                if (ImGui.BeginChild("##ImportDesignsList", new Vector2(0, designBoxHeight), true, ImGuiWindowFlags.AlwaysVerticalScrollbar))
                {
                    if (controller.Designs is { } designs)
                    {
                        foreach (var design in designs)
                        {
                            var isSelected = controller.SelectedDesignId == design.Id;

                            if (isSelected)
                            {
                                ImGui.PushStyleColor(ImGuiCol.Header, KinkLinkStyle.PrimaryColor);
                                if (ImGui.Selectable($"{design.Path}##{design.Id}", true))
                                {
                                    controller.SelectedDesignId = design.Id;
                                }
                                ImGui.PopStyleColor();
                            }
                            else
                            {
                                if (ImGui.Selectable($"{design.Path}##{design.Id}"))
                                {
                                    controller.SelectedDesignId = design.Id;
                                }
                            }
                        }
                    }

                    if (controller.Designs == null || controller.Designs.Count == 0)
                    {
                        ImGui.TextColored(ImGuiColors.DalamudGrey, "No designs found.");
                        ImGui.TextColored(ImGuiColors.DalamudGrey, "Make sure Glamourer is installed and has designs.");
                    }

                    ImGui.EndChild();
                }
            }
        );

        SharedUserInterfaces.ContentBox(
            "ImportButton",
            KinkLinkStyle.PanelBackground,
            false,
            () =>
            {
                var canImport = controller.SelectedDesignId != Guid.Empty;

                if (!canImport)
                {
                    ImGui.BeginDisabled();
                    ImGui.Button("Select a design to import", new Vector2(width, ImportButtonHeight));
                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Button("Import Design", new Vector2(width, ImportButtonHeight)))
                    {
                        ImportSelectedDesign();
                    }
                }
            }
        );
    }

    private void ImportSelectedDesign()
    {
        if (controller.SelectedDesignId == Guid.Empty)
            return;

        var design = controller.Designs?.FirstOrDefault(d => d.Id == controller.SelectedDesignId);
        if (design == null)
            return;

        var name = string.IsNullOrWhiteSpace(controller.EditedName) ? design.Name : controller.EditedName;
        var description = controller.EditedDescription ?? string.Empty;

        controller.ImportDesign(design, name, description);

        controller.EditedName = string.Empty;
        controller.EditedDescription = string.Empty;

        NotificationHelper.Success("Import", $"Imported {name} successfully");
    }
}
