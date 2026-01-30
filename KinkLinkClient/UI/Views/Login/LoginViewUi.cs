using System.Numerics;
using KinkLinkClient.Domain.Interfaces;
using KinkLinkClient.Managers;
using KinkLinkClient.Services;
using KinkLinkClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Microsoft.AspNetCore.SignalR.Client;

namespace KinkLinkClient.UI.Views.Login;

public class LoginViewUi(LoginViewUiController controller, NetworkService networkService) : IDrawable
{
    private const ImGuiInputTextFlags SecretInputFlags =
        ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.Password | ImGuiInputTextFlags.AutoSelectAll;

    public void Draw()
    {
        ImGui.BeginChild("LoginContent", Vector2.Zero, false, KinkLinkStyle.ContentFlags);

        ImGui.AlignTextToFramePadding();

        SharedUserInterfaces.ContentBox("LoginHeader", KinkLinkStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.BigTextCentered("Aether Remote");
            SharedUserInterfaces.TextCentered(Plugin.Version.ToString());
        });

        SharedUserInterfaces.ContentBox("LoginSecret", KinkLinkStyle.PanelBackground, true, () =>
        {
            var has_uid = false;
            var has_secret = false;

            SharedUserInterfaces.MediumText("Enter UID");
            if (ImGui.InputTextWithHint("##UidInput", "Uid", ref controller.ProfileUID, 10))
                has_uid = true;

            SharedUserInterfaces.MediumText("Enter Secret");
            if (ImGui.InputTextWithHint("##SecretInput", "Secret", ref controller.Secret, 120, SecretInputFlags))
                has_secret = true;

            var shouldConnect = has_uid && has_secret;
            ImGui.SameLine();
            if (networkService.Connecting)
            {
                ImGui.BeginDisabled();
                ImGui.Button("Connect");
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Connect"))
                    shouldConnect = true;
            }

            if (shouldConnect)
                controller.Connect();
            ImGui.Spacing();

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 0));

            ImGui.TextUnformatted("Need a secret? Join the");
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, KinkLinkStyle.DiscordBlue);
            var size = ImGui.CalcTextSize("discord");
            if (ImGui.Selectable("discord", false, ImGuiSelectableFlags.None, size))
                LoginViewUiController.OpenDiscordLink();

            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.TextUnformatted("to generate one.");

            ImGui.PopStyleVar();
        });

        ImGui.EndChild();
    }
}
