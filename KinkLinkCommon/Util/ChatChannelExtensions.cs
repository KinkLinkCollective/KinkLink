using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Domain.Enums.Permissions;

namespace KinkLinkCommon.Util;

/// <summary>
///     Extension methods for <see cref="ChatChannel"/>
/// </summary>
public static class ChatChannelExtensions
{
    /// <summary>
    ///     Converts a chat channel to a more readable format
    /// </summary>
    public static string Beautify(this ChatChannel chatChannel)
    {
        return chatChannel switch
        {
            ChatChannel.Linkshell => "LS",
            ChatChannel.FreeCompany => "Free Company",
            ChatChannel.CrossWorldLinkshell => "CWLS",
            ChatChannel.PvPTeam => "PvP Team",
            ChatChannel.Roleplay => "Chat Emote",
            _ => chatChannel.ToString()
        };
    }

    /// <summary>
    ///     Gets the chat command for the corresponding chat channel without the /
    /// </summary>
    public static string ChatCommand(this ChatChannel chatChannel)
    {
        return chatChannel switch
        {
            ChatChannel.Say => "s",
            ChatChannel.Yell => "y",
            ChatChannel.Shout => "sh",
            ChatChannel.Tell => "t",
            ChatChannel.Party => "p",
            ChatChannel.Alliance => "a",
            ChatChannel.FreeCompany => "fc",
            ChatChannel.Linkshell => "l",
            ChatChannel.CrossWorldLinkshell => "cwl",
            ChatChannel.PvPTeam => "pt",
            ChatChannel.Roleplay => "em",
            ChatChannel.Echo => "echo",
            _ => "Not Implemented"
        };
    }

    /// <summary>
    ///     Convert a chat channel to speak permissions
    /// </summary>
    public static GarblerChannels ToSpeakPermissions(this ChatChannel chatChannel, string? extra = null)
    {
        return chatChannel switch
        {
            ChatChannel.Say => GarblerChannels.Say,
            ChatChannel.Roleplay => GarblerChannels.Roleplay,
            ChatChannel.Echo => GarblerChannels.Echo,
            ChatChannel.Yell => GarblerChannels.Yell,
            ChatChannel.Shout => GarblerChannels.Shout,
            ChatChannel.Tell => GarblerChannels.Tell,
            ChatChannel.Party => GarblerChannels.Party,
            ChatChannel.Alliance => GarblerChannels.Alliance,
            ChatChannel.FreeCompany => GarblerChannels.FreeCompany,
            ChatChannel.PvPTeam => GarblerChannels.PvPTeam,
            ChatChannel.Linkshell => ConvertToLinkshell(GarblerChannels.Ls1, extra),
            ChatChannel.CrossWorldLinkshell => ConvertToLinkshell(GarblerChannels.Cwl1, extra),
            _ => GarblerChannels.None
        };
    }

    private static GarblerChannels ConvertToLinkshell(GarblerChannels starting, string? extra)
    {
        return int.TryParse(extra, out var number)
            ? (GarblerChannels)((int)starting << (number - 1))
            : GarblerChannels.None;
    }
}
