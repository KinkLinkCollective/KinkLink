using System.Numerics;
using KinkLinkClient.Domain.Interfaces;
using KinkLinkClient.Utils;
using Dalamud.Bindings.ImGui;

namespace KinkLinkClient.UI.Views.Interactions;

// TODO: This class needs to be implemented
public class InteractionsViewUi(InteractionsViewUiController controller) : IDrawable
{
    // Const
    private const int ActionButtonHeight = 40;

    public void Draw()
    {
        ImGui.BeginChild("InteractionsContent", Vector2.Zero, false, KinkLinkStyle.ContentFlags);

        var width = ImGui.GetWindowWidth();
        var padding = ImGui.GetStyle().WindowPadding;

        var begin = ImGui.GetCursorPosY();

        SharedUserInterfaces.ContentBox("#interactions", KinkLinkStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.BigTextCentered("interactions [TODO]");
            ImGui.TextUnformatted("This will be the menu with interactions Configuration");
        });

        var headerHeight = ImGui.GetCursorPosY() - begin;
        var contextBoxSize = new Vector2(0, ImGui.GetWindowHeight() - headerHeight - padding.X * 3 - ActionButtonHeight);
        // TODO: Implement the interactions configuration menu below
        ImGui.EndChild();
    }
}
