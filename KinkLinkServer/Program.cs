using System.Text;
using System.Reflection;
using KinkLinkCommon.Database;
using KinkLinkServer.Domain;
using KinkLinkServer.Domain.Interfaces;
using KinkLinkServer.Managers;
using KinkLinkServer.Services;
using KinkLinkServer.SignalR.Handlers;
using KinkLinkServer.SignalR.Hubs;
using MessagePack;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Npgsql;
using DbUp;

namespace KinkLinkServer;

// ReSharper disable once ClassNeverInstantiated.Global

public class Program
{
    private static void Main(string[] args)
    {
        // Attempt to load configuration values
        if (Configuration.Load() is not { } configuration)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Error cannot load Configuiration at {Configuration.ConfigurationPath}");
            Console.ResetColor();
            Environment.Exit(1);
            return;
        }

        // Migrate the database prior to building the WebApplication
        EnsureDatabase.For.PostgresqlDatabase(configuration.DatabaseConnectionString);

        var upgrader = DeployChanges.To.PostgresqlDatabase(configuration.DatabaseConnectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .LogToConsole()
            .Build();
        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine(result.Error);
            Console.ResetColor();
            Environment.Exit(1);
        }

        // Create service builder
        var builder = WebApplication.CreateBuilder(args);

        // Configuration Authentication and Authorization
        ConfigureJwtAuthentication(builder.Services, configuration);

        // Configure migration settings
        var configJson = File.ReadAllText(Configuration.ConfigurationPath);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddSignalR(options => options.EnableDetailedErrors = true)
            .AddMessagePackProtocol(options => options.SerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData));
        builder.Services.AddSingleton(configuration);
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
        // NOTE: HTTP endpoint is configured as traefik will be used as a reverse proxy
        // with TLS termination. This will never be exposed to the open internet.
        builder.WebHost.UseUrls("http://localhost:5006");
        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseRouting();

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
