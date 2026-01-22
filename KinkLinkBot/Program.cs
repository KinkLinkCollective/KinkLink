using KinkLinkBot.Configuration;
using KinkLinkBot.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace KinkLinkBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("[KinkLinkBot] Starting...");

        // Load configuration
        var configPath = args.Length > 0 ? args[0] : "config.yaml";
        var config = BotConfiguration.Load(configPath);

        if (config == null)
        {
            Console.WriteLine("[KinkLinkBot] Failed to load configuration. Exiting.");
            return;
        }

        // Set up Discord client
        var discordConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages,
            LogLevel = LogSeverity.Info
        };

        var client = new DiscordSocketClient(discordConfig);

        // Set up services
        var services = new ServiceCollection();
        services.AddSingleton(client);
        services.AddSingleton(_ => config);
        services.AddSingleton<RegistrationService>();
        services.AddSingleton<DiscordInteractionHandler>();

        var serviceProvider = services.BuildServiceProvider();

        // Get services
        var registrationService = serviceProvider.GetRequiredService<RegistrationService>();
        var interactionHandler = serviceProvider.GetRequiredService<DiscordInteractionHandler>();

        // Initialize interaction handler
        interactionHandler.Initialize();

        // Set up logging
        client.Log += LogAsync;

        // Login and start
        await client.LoginAsync(TokenType.Bot, config.Bot.Token);
        await client.StartAsync();

        // Check server health
        Console.WriteLine("[KinkLinkBot] Bot is ready. Press Ctrl+C to stop.");

        // Keep the bot running
        await Task.Delay(-1);
    }

    private static Task LogAsync(LogMessage message)
    {
        Console.WriteLine($"[Discord] [{message.Severity}] {message.Message}");
        return Task.CompletedTask;
    }
}
