using System;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using KinkLinkClient.Domain.Interfaces;
using KinkLinkClient.Services;
using KinkLinkClient.Utils;

namespace KinkLinkClient.UI.Views.Wardrobe;

public partial class WardrobeViewUi(WardrobeViewUiController controller) : IDrawable
{
    private const int ImportButtonHeight = 40;
    private const int HeaderMinHeight = 60;
    private const int StatusBarHeight = 50;

    private WardrobeService wardrobeService => controller.WardrobeService;

    public void Draw()
    {
        ImGui.BeginChild("WardrobeContent", Vector2.Zero, false, KinkLinkStyle.ContentFlags);

        DrawHeader();

        switch (controller.CurrentView)
        {
            case SubView.List:
                DrawListView();
                break;
            case SubView.Import:
                DrawImportView();
                break;
            case SubView.Editor:
                DrawEditorView();
                break;
        }

        DrawStatusBar();

        ImGui.EndChild();
        ImGui.SameLine();
    }

    private void DrawHeader()
    {
        SharedUserInterfaces.ContentBox(
            "Wardrobe",
            KinkLinkStyle.PanelBackground,
            true,
            () =>
            {
                var padding = ImGui.GetStyle().WindowPadding;
                var width = ImGui.GetWindowWidth() - padding.X * 2;

                var isSubView = controller.CurrentView != SubView.List;
                var backButtonWidth = isSubView ? 60 : 0;
                var importButtonWidth = controller.CurrentView == SubView.List ? 80 : 0;
                var titleText = controller.CurrentView switch
                {
                    SubView.Import => "Wardrobe [Import]",
                    SubView.Editor => "Wardrobe [Edit]",
                    _ => "Wardrobe",
                };

                if (isSubView)
                {
                    if (ImGui.Button("Back", new Vector2(backButtonWidth, 35)))
                    {
                        controller.CurrentView = SubView.List;
                    }
                    ImGui.SameLine();
                }

                var titleWidth = ImGui.CalcTextSize(titleText).X;
                var titleStartX = isSubView ? backButtonWidth + padding.X : 0;

                ImGui.SetCursorPosX(titleStartX + (width - titleStartX - titleWidth) * 0.5f);
                SharedUserInterfaces.MediumText(titleText);

                var currentHeight = ImGui.GetCursorPosY();
                var minHeight = HeaderMinHeight - padding.Y;
                if (currentHeight < minHeight)
                    ImGui.SetCursorPosY(minHeight);
            }
        );
    }

    private void DrawStatusBar()
    {
        var activeSet = wardrobeService.ActiveSet;
        if (activeSet == null)
            return;

        var padding = ImGui.GetStyle().WindowPadding;
        var width = ImGui.GetWindowWidth() - padding.X * 2;

        SharedUserInterfaces.ContentBox(
            "ActiveSetStatus",
            KinkLinkStyle.PanelBackground,
            false,
            () =>
            {
                ImGui.Text($"Currently Applied: {activeSet.Name}");

                ImGui.SameLine(width - 80);

                if (ImGui.Button("Remove", new Vector2(70, 24)))
                {
                    _ = RemoveActiveSetWithErrorHandling();
                }
            }
        );
    }

    private async Task RemoveActiveSetWithErrorHandling()
    {
        try
        {
            if (!wardrobeService.IsGlamourerApiAvailable)
            {
                NotificationHelper.Error("Remove Set", "Glamourer is not available.");
                return;
            }

            await controller.RemoveActiveSetAsync();
            NotificationHelper.Success("Remove Set", "Removed active set");
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "Failed to remove active set");
            NotificationHelper.Error("Remove Set", "Failed to remove set. Check logs for details.");
        }
    }
}
