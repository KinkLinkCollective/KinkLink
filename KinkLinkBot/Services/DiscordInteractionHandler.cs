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
    private readonly ulong _channelId;
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
        _channelId = config.Bot.ChannelId;
        _guildId = config.Bot.GuildId;
        _logger = logger;
    }

    public void Initialize()
    {
        _client.ButtonExecuted += HandleButtonInteractionAsync;
        _client.ModalSubmitted += HandleModalSubmissionAsync;
        _client.Ready += EnsureRegistrationPromptExistsAsync;
    }

    public async Task EnsureRegistrationPromptExistsAsync()
    {
        try
        {
            var guild = _client.GetGuild(_guildId);
            if (guild == null)
            {
                _logger.LogError($"Guild {_guildId} not found during startup");
                return;
            }

            var channel = guild.GetTextChannel(_channelId);
            if (channel == null)
            {
                _logger.LogError($"Channel {_channelId} not found in guild {_guildId} during startup");
                return;
            }

            var existingPinnedMessages = await channel.GetPinnedMessagesAsync();
            var hasRegistrationPrompt = existingPinnedMessages.Any(IsRegistrationPromptMessage);

            if (!hasRegistrationPrompt)
            {
                _logger.LogInformation($"No registration prompt found in channel {channel.Id}, creating new one");
                await CreatePrompt();
            }
            else
            {
                _logger.LogInformation($"Registration prompt already exists in channel {channel.Id}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring registration prompt exists during startup");
        }
    }

    private bool IsRegistrationPromptMessage(IMessage message)
    {
        return message.Embeds?.Any(embed =>
            embed.Title?.Contains("KinkLink Registration", StringComparison.OrdinalIgnoreCase) == true) == true;
    }


    public async Task<RegistrationResponse> CreatePrompt()
    {
        try
        {
            // Get the guild from the client
            var guild = _client.GetGuild(_guildId);
            if (guild == null)
            {
                _logger.LogError($"Guild {_guildId} not found");
                return new RegistrationResponse
                {
                    Success = false,
                    ErrorMessage = "Guild not found"
                };
            }

            // Get the specific channel from configuration
            var channel = guild.GetTextChannel(_channelId);
            if (channel == null)
            {
                _logger.LogError($"Channel {_channelId} not found in guild {_guildId}");
                return new RegistrationResponse
                {
                    Success = false,
                    ErrorMessage = "Registration channel not found"
                };
            }

            // Create an embed for the registration prompt
            var embed = new EmbedBuilder
            {
                Title = "🔗 KinkLink Registration",
                Description = "Welcome to KinkLink! To get started, please register your account.",
                Color = Color.Blue,
            };

            embed.AddField("📝 Registration Steps",
                "1. Click the button below to start registration\n" +
                "2. Follow the prompts to create your unique secret key and first UID\n" +
                "3. Use the provided secret key to connect with KinkLink in game",
                inline: false);

            embed.AddField("📋 Community Rules",
                "• **18+ Player Only**: This is a legal thing, minors cannot consent, therefore they are not allowed\n" +
                "• **18+ Characters Only**: This is a values thing, even if you are 18+ IRL, using child avatars is uncomfortable so please don't when using this service\n" +
                "• **Safe Space**: Zero tolerance for harassment, bullying, or stalking behavior\n" +
                "• **Understand Consent**: The core of BDSM is consent, be sure you understand what it means for you and your partner(s). Respect boundaries and limits\n" +
                "• **Be Respectful**: We're all people, do give common courtesy and respect",
                inline: false);

            embed.AddField("🔐 Privacy & Security",
                "• Your Discord ID number (not your username or anything like that) will be stored on the server for *account management purposes only*." +
                "• UIDs provide anonymity in-game\n" +
                "• You can delete your account at any time",
                inline: false);

            embed.AddField("❓ Need Help?",
                "Contact an administrator if you need assistance with registration.",
                inline: false);

            embed.WithFooter("KinkLink Bot • Secure Registration System");
            embed.WithCurrentTimestamp();

            // Create a button for user interaction
            var buttonBuilder = new ComponentBuilder()
                .WithButton("🚀 Start Registration", "register_start", ButtonStyle.Primary);

            // Send the embed message to the channel
            var message = await channel.SendMessageAsync(
                embed: embed.Build(),
                components: buttonBuilder.Build());

            try
            {
                await message.PinAsync();
                _logger.LogInformation($"Registration prompt sent and pinned to channel {channel.Id} in guild {guild.Id}");
            }
            catch (Exception pinEx)
            {
                _logger.LogWarning(pinEx, $"Failed to pin registration message to channel {channel.Id} in guild {guild.Id}");
            }

            return new RegistrationResponse
            {
                Success = true,
                ErrorMessage = null,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating registration prompt");
            return new RegistrationResponse
            {
                Success = false,
                ErrorMessage = $"Failed to create registration prompt: {ex.Message}"
            };
        }
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
            case "create_first_uid":
                await HandleCreateUIDAsync(component);
                break;
            case "skip_uid_creation":
                await HandleSkipUIDCreationAsync(component);
                break;
            case "remove_account":
                await HandleAccountRemovalAsync(component);
                break;
            case "create_uid":
                await HandleCreateUIDAsync(component);
                break;
            case "remove_uid":
                await HandleRemoveUIDAsync(component);
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
                    Description = $"Your KinkLink account has been created!\n\n **Your Secret is:** `{response.Secret}`\n\n Be sure to save it, we cannot return it to you.",
                    Color = Color.Green,
                    Footer = new EmbedFooterBuilder { Text = "Keep your secret safe!" }
                };

                embed.AddField("🎯 Create Your First UID?",
                    "A UID is your public identity that others use to connect with you. You can create one now or skip and create it later.",
                    inline: false);

                embed.AddField("⚠️ Important",
                    "• Your UID is public and used to connect with others\n" +
                    "• Your secret is private and authenticates your connection\n" +
                    "• Never share your secret with anyone!",
                    inline: false);

                // Create buttons for creating first UID or skipping
                var buttonBuilder = new ComponentBuilder()
                    .WithButton("🎭 Create First UID", "create_first_uid", ButtonStyle.Primary)
                    .WithButton("⏭️ Skip for Now", "skip_uid_creation", ButtonStyle.Secondary);

                await component.RespondAsync(embed: embed.Build(), components: buttonBuilder.Build(), ephemeral: true);
                _logger.LogInformation($"User {user.Username} ({user.Id}) successfully registered your discord account ");
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

    private async Task HandleAccountRemovalAsync(SocketMessageComponent component)
    {
        try
        {
            // Show confirmation modal for account deletion
            var modal = new ModalBuilder()
                .WithTitle("Confirm Account Deletion")
                .WithCustomId("confirm_account_removal")
                .AddTextInput("Type 'DELETE' to confirm", "confirmation", TextInputStyle.Short,
                    placeholder: "This action is irreversible!", required: true);

            await component.RespondWithModalAsync(modal.Build());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling account removal prompt for user {UserId}", component.User.Id);
            await component.RespondAsync("An unexpected error occurred. Please try again later.", ephemeral: true);
        }
    }

    private async Task HandleCreateUIDAsync(SocketMessageComponent component)
    {
        try
        {
            var response = await _registrationService.CreateUID(component.User.Id);

            if (response.Success)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("✅ UID Created Successfully")
                    .WithDescription($"Your new UID has been generated. Use this UID to connect to the game client.\n\n**Secret Key:** `{response.Secret}`")
                    .WithColor(Color.Green)
                    .AddField("Next Steps", "1. Open KinkLink plugin in FFXIV\n2. Enter your UID and secret key\n3. Configure your profile settings")
                    .WithFooter("Keep your secret key secure!")
                    .Build();

                await component.RespondAsync(embed: embed, ephemeral: true);
            }
            else
            {
                var embed = new EmbedBuilder()
                    .WithTitle("❌ UID Creation Failed")
                    .WithDescription(response.ErrorMessage ?? "Unknown error occurred")
                    .WithColor(Color.Red)
                    .Build();

                await component.RespondAsync(embed: embed, ephemeral: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UID creation for user {UserId}", component.User.Id);
            await component.RespondAsync("An unexpected error occurred while creating your UID. Please try again later.", ephemeral: true);
        }
    }

    private async Task HandleSkipUIDCreationAsync(SocketMessageComponent component)
    {
        try
        {
            var embed = new EmbedBuilder
            {
                Title = "✅ Registration Complete",
                Description = "You can create your UID later through the bot commands or by registering again.",
                Color = Color.Green
            };

            embed.AddField("🔗 Next Steps",
                "1. Open KinkLink plugin in FFXIV\n" +
                "2. Enter your Secret when prompted\n" +
                "3. Create your UID when you're ready",
                inline: false
            );

            embed.AddField("💡 Tip",
                "You can create multiple UIDs later for different characters, roles, or OC.",
                inline: false
            );

            await component.RespondAsync(embed: embed.Build(), ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling skip UID creation for user {UserId}", component.User.Id);
            await component.RespondAsync("An unexpected error occurred. Please try again later.", ephemeral: true);
        }
    }

    private async Task HandleRemoveUIDAsync(SocketMessageComponent component)
    {
        try
        {
            // Create a modal for UID deletion
            var modal = new ModalBuilder()
                .WithTitle("Remove UID")
                .WithCustomId("confirm_uid_removal")
                .AddTextInput("Enter UID to remove", "uid", TextInputStyle.Short,
                    placeholder: "Enter the UID you want to delete", required: true)
                .AddTextInput("Type 'DELETE' to confirm", "confirmation", TextInputStyle.Short,
                    placeholder: "This action is irreversible!", required: true);

            await component.RespondWithModalAsync(modal.Build());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UID removal prompt for user {UserId}", component.User.Id);
            await component.RespondAsync("An unexpected error occurred. Please try again later.", ephemeral: true);
        }
    }

    private async Task HandleModalSubmissionAsync(SocketModal modal)
    {
        // Only handle interactions in the configured guild
        if (modal.GuildId != _guildId)
            return;

        switch (modal.Data.CustomId)
        {
            case "confirm_account_removal":
                await HandleAccountRemovalConfirmationAsync(modal);
                break;
            case "confirm_uid_removal":
                await HandleUIDRemovalConfirmationAsync(modal);
                break;
        }
    }

    private async Task HandleAccountRemovalConfirmationAsync(SocketModal modal)
    {
        try
        {
            var confirmation = modal.Data.Components.FirstOrDefault(x => x.CustomId == "confirmation")?.Value;

            if (confirmation?.Trim().ToUpper() != "DELETE")
            {
                await modal.RespondAsync("Confirmation text incorrect. Account deletion cancelled.", ephemeral: true);
                return;
            }

            var response = await _registrationService.RemoveAccount(modal.User.Id);

            if (response.Success)
            {
                var embed = new EmbedBuilder
                {
                    Title = "✅ Account Deleted",
                    Description = "Your KinkLink account and all associated data have been permanently deleted.",
                    Color = Color.Red
                };

                await modal.RespondAsync(embed: embed.Build(), ephemeral: true);
                _logger.LogInformation($"User {modal.User.Username} ({modal.User.Id}) successfully deleted their account");
            }
            else
            {
                var errorEmbed = new EmbedBuilder
                {
                    Title = "❌ Account Deletion Failed",
                    Description = response.ErrorMessage ?? "An unknown error occurred",
                    Color = Color.Red
                };

                await modal.RespondAsync(embed: errorEmbed.Build(), ephemeral: true);
                _logger.LogWarning($"Account deletion failed for user {modal.User.Username} ({modal.User.Id}): {response.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling account removal confirmation for user {UserId}", modal.User.Id);
            await modal.RespondAsync("An unexpected error occurred while deleting your account. Please try again later.", ephemeral: true);
        }
    }

    private async Task HandleUIDRemovalConfirmationAsync(SocketModal modal)
    {
        try
        {
            var uid = modal.Data.Components.FirstOrDefault(x => x.CustomId == "uid")?.Value?.Trim();
            var confirmation = modal.Data.Components.FirstOrDefault(x => x.CustomId == "confirmation")?.Value?.Trim().ToUpper();

            if (confirmation != "DELETE")
            {
                await modal.RespondAsync("Confirmation text incorrect. UID deletion cancelled.", ephemeral: true);
                return;
            }

            if (string.IsNullOrEmpty(uid))
            {
                await modal.RespondAsync("UID is required.", ephemeral: true);
                return;
            }

            var response = await _registrationService.DeleteUID(modal.User.Id, uid);

            if (response.Success)
            {
                var embed = new EmbedBuilder
                {
                    Title = "✅ UID Deleted",
                    Description = $"UID `{uid}` has been permanently deleted.",
                    Color = Color.Green
                };

                await modal.RespondAsync(embed: embed.Build(), ephemeral: true);
                _logger.LogInformation($"User {modal.User.Username} ({modal.User.Id}) successfully deleted UID {uid}");
            }
            else
            {
                var errorEmbed = new EmbedBuilder
                {
                    Title = "❌ UID Deletion Failed",
                    Description = response.ErrorMessage ?? "An unknown error occurred",
                    Color = Color.Red
                };

                await modal.RespondAsync(embed: errorEmbed.Build(), ephemeral: true);
                _logger.LogWarning($"UID deletion failed for user {modal.User.Username} ({modal.User.Id}), UID {uid}: {response.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UID removal confirmation for user {UserId}", modal.User.Id);
            await modal.RespondAsync("An unexpected error occurred while deleting your UID. Please try again later.", ephemeral: true);
        }
    }

    // TODO: Add method to regenerate the secret key so long as your account has previously registered.
}
