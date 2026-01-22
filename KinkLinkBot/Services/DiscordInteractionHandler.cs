using KinkLinkBot.Configuration;
using KinkLinkBot.Domain.Models;
using Discord;
using Discord.WebSocket;

namespace KinkLinkBot.Services;

/// <summary>
///     Handles Discord slash command interactions
/// </summary>
public class DiscordInteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly RegistrationService _registrationService;
    private readonly ulong _adminRoleId;
    private readonly ulong _guildId;

    public DiscordInteractionHandler(
        DiscordSocketClient client,
        RegistrationService registrationService,
        BotConfiguration config)
    {
        _client = client;
        _registrationService = registrationService;
        _adminRoleId = config.Bot.AdminRoleId;
        _guildId = config.Bot.GuildId;
    }

    public void Initialize()
    {
    }
}
