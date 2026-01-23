using System.Numerics;
using KinkLinkClient.Domain.Interfaces;
using KinkLinkClient.Utils;
using Dalamud.Bindings.ImGui;

namespace KinkLinkClient.UI.Views.History;

public class HistoryViewUi(HistoryViewUiController controller) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("PermissionContent", Vector2.Zero, false, ImGuiWindowFlags.NoBackground);

        SharedUserInterfaces.ContentBox("HistorySearch", KinkLinkStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("History");

            if (ImGui.InputTextWithHint("##Search", "Search", ref controller.Search, 200))
                controller.Logs.UpdateSearchTerm(controller.Search);
        });

        SharedUserInterfaces.ContentBox("HistoryLog", KinkLinkStyle.PanelBackground, false, () =>
        {
            for (var i = controller.Logs.List.Count - 1; i >= 0; i--)
            {
                var log = controller.Logs.List[i];
                ImGui.TextUnformatted($"[{log.TimeStamp.ToLongTimeString()}] {log.Message}");
            }
        });

        ImGui.EndChild();
    }
}
