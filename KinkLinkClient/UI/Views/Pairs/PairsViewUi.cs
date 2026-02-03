using System.Numerics;
using KinkLinkClient.Domain.Interfaces;
using KinkLinkClient.Utils;
using Dalamud.Bindings.ImGui;

namespace KinkLinkClient.UI.Views.Pairs;

// TODO: This class needs to be implemented
public class PairsViewUi(PairsViewUiController controller) : IDrawable
{
    // Const
    private const int ActionButtonHeight = 40;

    public void Draw()
    {
        ImGui.BeginChild("PairsContent", Vector2.Zero, false, KinkLinkStyle.ContentFlags);

        var width = ImGui.GetWindowWidth();
        var padding = new Vector2(ImGui.GetStyle().WindowPadding.X, 0);

        var begin = ImGui.GetCursorPosY();

        SharedUserInterfaces.ContentBox("Pairs [TODO]", KinkLinkStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.BigTextCentered("This will be the menu with Pairs Configuration");
        });

        var headerHeight = ImGui.GetCursorPosY() - begin;
        var contextBoxSize = new Vector2(0, ImGui.GetWindowHeight() - headerHeight - padding.X * 3 - ActionButtonHeight);
        // TODO: Implement the pairs configuration menu below
    }
}