using System;
using System.IO;
using KinkLinkClient.Managers;
using KinkLinkClient.Services;
using KinkLinkClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Utility;
using System.Collections.Generic;

namespace KinkLinkClient.UI.Views.Login;

public class LoginViewUiController : IDisposable
{
    // Injected
    private readonly NetworkService _networkService;
    private readonly LoginManager _loginManager;

    /// <summary>
    ///     User inputted secret
    /// </summary>
    public string Secret = string.Empty;
    public string ProfileUID = string.Empty;

    public LoginViewUiController(NetworkService networkService, LoginManager loginManager)
    {
        _networkService = networkService;
        _loginManager = loginManager;
        _loginManager.LoginFinished += OnLoginFinished;
    }

    public async void Connect()
    {
        try
        {
            // Only save if the configuration is set
            if (Plugin.Configuration is null || Plugin.CharacterConfiguration is null)
                return;

            // Don't save if the string is empty
            if (Secret == string.Empty)
                return;

            // Set the secret
            Plugin.Configuration.SecretKey = this.Secret;
            Plugin.CharacterConfiguration.ProfileUID = this.ProfileUID;

            // Save the configuration
            await Plugin.Configuration.Save().ConfigureAwait(false);
            await Plugin.CharacterConfiguration.Save().ConfigureAwait(false);
            Plugin.Log.Error($"{Plugin.Configuration.SecretKey} {Plugin.CharacterConfiguration.ProfileUID}");
            // Try to connect to the server
            await _networkService.StartAsync();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    // TODO: This needs to redirect to the actual server. Actually, IDK if I will make it public?
    public static void OpenDiscordLink() => Util.OpenLink("https://discord.com/invite/aetherremote");

    private void OnLoginFinished()
    {
        if (Plugin.CharacterConfiguration is null)
            return;

        ProfileUID = Plugin.CharacterConfiguration.ProfileUID;
    }

    public void Dispose()
    {
        _loginManager.LoginFinished -= OnLoginFinished;
        GC.SuppressFinalize(this);
    }
}
