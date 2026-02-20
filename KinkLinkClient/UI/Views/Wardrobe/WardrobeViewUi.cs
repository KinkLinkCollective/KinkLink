using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using KinkLinkClient.Domain.Interfaces;
using KinkLinkClient.Utils;

namespace KinkLinkClient.UI.Views.Wardrobe;

public partial class WardrobeViewUi(WardrobeViewUiController controller) : IDrawable
{
    private const int ImportButtonHeight = 40;
    private const int HeaderMinHeight = 60;

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
}
