using System;
using System.Collections.Generic;
using KinkLinkClient.Domain;
using KinkLinkClient.Managers;
using KinkLinkClient.Services;
using KinkLinkClient.Utils;
using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Domain.Enums.Permissions;
using KinkLinkCommon.Domain.Network;
using KinkLinkCommon.Domain.Network.Emote;

namespace KinkLinkClient.UI.Views.Emote;

/// <summary>
///     Handles events from the <see cref="EmoteViewUi"/>
/// </summary>
public class EmoteViewUiController(EmoteService emoteService, NetworkService networkService, SelectionManager selectionManager)
{
    public readonly ListFilter<string> EmotesListFilter = new(emoteService.Emotes, FilterEmote);
    public string EmoteSelection = string.Empty;
    public bool DisplayLogMessage = false;

    private static bool FilterEmote(string emote, string searchTerm) => emote.Contains(searchTerm);

    /// <summary>
    ///     Handles the "send button" from the Ui
    /// </summary>
    public async void Send()
    {
        try
        {
            if (emoteService.Emotes.Contains(EmoteSelection) is false)
                return;

            var request = new EmoteRequest(selectionManager.GetSelectedFriendCodes(), EmoteSelection, DisplayLogMessage);
            var response = await networkService.InvokeAsync<ActionResponse>(HubMethod.Emote, request).ConfigureAwait(false);
            if (response.Result is ActionResponseEc.Success)
                EmoteSelection = string.Empty;

            ActionResponseParser.Parse("Emote", response);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Unable to emote, {e.Message}");
        }
    }

    /// <summary>
    ///     Calculates the friends who you lack correct permissions to send to
    /// </summary>
    /// <returns></returns>
    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in selectionManager.Selected)
        {
            thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        return thoseWhoYouLackPermissionsFor;
    }
}
