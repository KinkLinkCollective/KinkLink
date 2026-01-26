using System.Net;
using System.Text;
using KinkLinkCommon.Database;
using KinkLinkServer.Domain;
using KinkLinkServer.Domain.Interfaces;
using KinkLinkServer.Managers;
using KinkLinkServer.Services;
using KinkLinkServer.SignalR.Handlers;
using KinkLinkServer.SignalR.Hubs;
using MessagePack;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace KinkLinkServer;

// ReSharper disable once ClassNeverInstantiated.Global

public class Program
{
    private static void Main(string[] args)
    {
        // Attempt to load configuration values
        if (Configuration.Load() is not { } configuration)
        {
            Environment.Exit(1);
            return;
        }

        // Create service builder
        var builder = WebApplication.CreateBuilder(args);

        // Configuration Authentication and Authorization
        ConfigureJwtAuthentication(builder.Services, configuration);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddSignalR(options => options.EnableDetailedErrors = true)
            .AddMessagePackProtocol(options => options.SerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData));
        builder.Services.AddSingleton(configuration);

        // Run migrations on startup
        builder.Services.AddSingleton<MigrationManager>();
        builder.Services.AddHostedService(sp =>
        {
            var migrator = sp.GetRequiredService<MigrationManager>();
            var logger = sp.GetRequiredService<ILogger<MigrationManager>>();
            var connection = new NpgsqlConnection(configuration.DatabaseConnectionString);
            connection.Open();
            return new MigrationHostedService(migrator, connection, logger);
        });

        // Register database service - DatabaseService handles its own QueriesSql creation
        builder.Services.AddSingleton<DatabaseService>();

        // Services
        builder.Services.AddSingleton<IPresenceService, PresenceService>();
        builder.Services.AddSingleton<IRequestLoggingService, RequestLoggingService>();

        // Managers
        builder.Services.AddSingleton<IForwardedRequestManager, ForwardedRequestManager>();

        // Handles
        builder.Services.AddSingleton<OnlineStatusUpdateHandler>();
        builder.Services.AddSingleton<AddFriendHandler>();
        builder.Services.AddSingleton<CustomizePlusHandler>();
        builder.Services.AddSingleton<EmoteHandler>();
        builder.Services.AddSingleton<GetAccountDataHandler>();
        builder.Services.AddSingleton<HonorificHandler>();
        builder.Services.AddSingleton<MoodlesHandler>();
        builder.Services.AddSingleton<RemoveFriendHandler>();
        builder.Services.AddSingleton<SpeakHandler>();
        builder.Services.AddSingleton<UpdateFriendHandler>();

        builder.WebHost.UseUrls("https://localhost:5006");
        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseRouting();
        // app.UseHttpsRedirection(); // Disabled for Traefik development setup

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHub<PrimaryHub>("/primaryHub");
        app.MapControllers();

        app.Run();
    }

    private static void ConfigureJwtAuthentication(IServiceCollection services, Configuration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.SigningKey)),
            };
        });
    }
}

/// <summary>
/// Background service that runs database migrations on startup
/// </summary>
public class MigrationHostedService : BackgroundService
{
    private readonly MigrationManager _migrator;
    private readonly NpgsqlConnection _connection;
    private readonly ILogger<MigrationManager> _logger;

    public MigrationHostedService(
        MigrationManager migrator,
        NpgsqlConnection connection,
        ILogger<MigrationManager> logger)
    {
        _migrator = migrator;
        _connection = connection;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var migrationsPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "KinkLinkCommon",
                "Database",
                "sql",
                "migrations");

            var migrationFiles = MigrationManager.GetMigrationFiles(migrationsPath);
            await _migrator.RunMigrationsAsync(migrationFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run database migrations");
            throw;
        }
    }
}
