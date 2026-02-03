using System.Numerics;
using KinkLinkClient.Domain.Interfaces;
using KinkLinkClient.Utils;
using Dalamud.Bindings.ImGui;

// ReSharper disable RedundantBoolCompare

namespace KinkLinkClient.UI.Views.Gags;

// TODO: This class needs to be implemented
public class GagsViewUi(GagsViewUiController controller) : IDrawable
{
    // Const
    private const int SendGagsButtonHeight = 40;

    public void Draw()
    {
        ImGui.BeginChild("GagsContent", Vector2.Zero, false, KinkLinkStyle.ContentFlags);

        var width = ImGui.GetWindowWidth();
        var padding = ImGui.GetStyle().WindowPadding;

        var begin = ImGui.GetCursorPosY();

        SharedUserInterfaces.ContentBox("#gags", KinkLinkStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.BigTextCentered("Gags [TODO]");
            ImGui.TextUnformatted("This will be the menu with gags Configuration");
        });

        var headerHeight = ImGui.GetCursorPosY() - begin;
        var chatContextBoxSize = new Vector2(0, ImGui.GetWindowHeight() - headerHeight - padding.X * 3 - SendGagsButtonHeight);
        // TODO: Implement the gag configuraiton menu below
        ImGui.EndChild();
    }
}
