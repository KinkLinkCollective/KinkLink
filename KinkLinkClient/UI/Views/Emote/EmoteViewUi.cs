using System.Numerics;
using KinkLinkClient.Domain.Interfaces;
using KinkLinkClient.Managers;
using KinkLinkClient.Services;
using KinkLinkClient.UI.Components.Friends;
using KinkLinkClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace KinkLinkClient.UI.Views.Emote;

public class EmoteViewUi(
    PairsListComponentUi friendsList,
    EmoteViewUiController controller,
    CommandLockoutService commandLockoutService,
    SelectionManager selectionManager) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("EmoteContent", KinkLinkStyle.ContentSize, false, KinkLinkStyle.ContentFlags);

        switch (selectionManager.Selected.Count)
        {
            case 0:
                SharedUserInterfaces.ContentBox("EmoteSelectMoreFriends", KinkLinkStyle.PanelBackground, true,
                    () =>
                    {
                        SharedUserInterfaces.TextCentered("You must select at least one friend");
                    });

                ImGui.EndChild();
                ImGui.SameLine();
                friendsList.Draw();
                return;

            case > 3:
                SharedUserInterfaces.ContentBox("EmoteLimitedSelection", KinkLinkStyle.PanelBackground, true,
                    () =>
                    {
                        SharedUserInterfaces.TextCentered("You may only select 3 friends for in game functions");
                    });

                ImGui.EndChild();
                ImGui.SameLine();
                friendsList.Draw();
                return;
        }

        SharedUserInterfaces.ContentBox("EmoteOptions", KinkLinkStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Options");
            ImGui.Checkbox("Display log message?", ref controller.DisplayLogMessage);
        });

        var friendsLackingPermissions = controller.GetFriendsLackingPermissions();
        if (friendsLackingPermissions.Count is not 0)
        {
            SharedUserInterfaces.ContentBox("EmoteLackingPermissions", KinkLinkStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.MediumText("Lacking Permissions", ImGuiColors.DalamudYellow);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationTriangle, ImGuiColors.DalamudYellow);
                SharedUserInterfaces.Tooltip("Commands send to these people will not be processed");
                ImGui.TextWrapped(string.Join(", ", friendsLackingPermissions));
            });
        }

        SharedUserInterfaces.ContentBox("EmoteSend", KinkLinkStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText("Emote");

            var width = ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X * 2;
            SharedUserInterfaces.ComboWithFilter("##EmoteSelector", "Search emotes", ref controller.EmoteSelection,
                width, controller.EmotesListFilter);

            ImGui.Spacing();

            if (commandLockoutService.IsLocked)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Send", new Vector2(width, 0));
                ImGui.EndDisabled();
            }
            else
            {
                // If the button is not pressed, exit
                if (ImGui.Button("Send", new Vector2(width, 0)) is false)
                    return;

                commandLockoutService.Lock();
                controller.Send();
            }
        });

        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}
