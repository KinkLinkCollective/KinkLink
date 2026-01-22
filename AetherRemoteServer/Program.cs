using System.Net;
using System.Text;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.SignalR.Handlers;
using AetherRemoteServer.SignalR.Hubs;
using MessagePack;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AetherRemoteServer;

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

        // Services
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<IPresenceService, PresenceService>();
        builder.Services.AddSingleton<IRequestLoggingService, RequestLoggingService>();

        // Managers
        builder.Services.AddSingleton<IForwardedRequestManager, ForwardedRequestManager>();

        // Handles
        builder.Services.AddSingleton<OnlineStatusUpdateHandler>();
        builder.Services.AddSingleton<AddFriendHandler>();
        // builder.Services.AddSingleton<CustomizePlusHandler>();
        builder.Services.AddSingleton<EmoteHandler>();
        builder.Services.AddSingleton<GetAccountDataHandler>();
        builder.Services.AddSingleton<HonorificHandler>();
        builder.Services.AddSingleton<MoodlesHandler>();
        builder.Services.AddSingleton<RemoveFriendHandler>();
        builder.Services.AddSingleton<SpeakHandler>();
        builder.Services.AddSingleton<TransformHandler>();
        builder.Services.AddSingleton<TwinningHandler>();
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
