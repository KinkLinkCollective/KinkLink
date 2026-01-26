using Microsoft.Extensions.Logging;
using KinkLinkBot.Configuration;
using KinkLinkBot.Domain.Models;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;

namespace KinkLinkBot.Services;

/// <summary>
///     Handles Discord button interactions for registration
/// </summary>
public class DiscordInteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly RegistrationService _registrationService;
    private readonly ulong _adminRoleId;
    private readonly ulong _guildId;
    private readonly ILogger<DiscordInteractionHandler> _logger;

    public DiscordInteractionHandler(
        DiscordSocketClient client,
        RegistrationService registrationService,
        BotConfiguration config,
        ILogger<DiscordInteractionHandler> logger)
    {
        _client = client;
        _registrationService = registrationService;
        _adminRoleId = config.Bot.AdminRoleId;
        _guildId = config.Bot.GuildId;
        _logger = logger;
    }

    public void Initialize()
    {
        // Hook into the button interaction event
        _client.ButtonExecuted += HandleButtonInteractionAsync;
    }

    private async Task HandleButtonInteractionAsync(SocketMessageComponent component)
    {
        // Only handle interactions in the configured guild
        if (component.GuildId != _guildId)
            return;

        switch (component.Data.CustomId)
        {
            case "register_start":
                await HandleRegistrationStartAsync(component);
                break;
        }
    }

    private async Task HandleRegistrationStartAsync(SocketMessageComponent component)
    {
        try
        {
            var user = component.User as SocketGuildUser;
            if (user == null)
            {
                await component.RespondAsync("Error: Unable to identify user.", ephemeral: true);
                return;
            }

            // Call the registration service to register the user
            var response = await _registrationService.RegisterUserAccount(user.Id);

            if (response.Success)
            {
                var embed = new EmbedBuilder
                {
                    Title = "✅ Registration Successful",
                    Description = $"Your KinkLink account has been created!\n\n**UID:** `{response.UID}`\n**Secret:** `{response.Secret}`",
                    Color = Color.Green,
                    Footer = new EmbedFooterBuilder { Text = "Keep your secret safe!" }
                };

                embed.AddField("🔗 Next Steps",
                    "1. Open KinkLink plugin in FFXIV\n" +
                    "2. Enter your UID when prompted\n" +
                    "3. Save your configuration",
                    inline: false);

                embed.AddField("⚠️ Important",
                    "• Your UID is public and used to connect with others\n" +
                    "• Your secret is private and authenticates your connection\n" +
                    "• Never share your secret with anyone!",
                    inline: false);

                await component.RespondAsync(embed: embed.Build(), ephemeral: true);
                _logger.LogInformation($"User {user.Username} ({user.Id}) successfully registered with UID {response.UID}");
            }
            else
            {
                var errorEmbed = new EmbedBuilder
                {
                    Title = "❌ Registration Failed",
                    Description = response.ErrorMessage ?? "An unknown error occurred",
                    Color = Color.Red
                };

                await component.RespondAsync(embed: errorEmbed.Build(), ephemeral: true);
                _logger.LogWarning($"Registration failed for user {user.Username} ({user.Id}): {response.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling registration start for user {UserId}", component.User.Id);
            await component.RespondAsync("An unexpected error occurred. Please try again later.", ephemeral: true);
        }
    }
}
