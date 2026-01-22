using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using KinkLinkBot.Configuration;
using KinkLinkBot.Domain.Models;

namespace KinkLinkBot.Services;

/// <summary>
///     Service for communicating with the AetherRemote server API
/// </summary>
public class RegistrationService
{
    public RegistrationService(BotConfiguration config)
    {
        // TODO: Add the reference to the postgres database
    }

    /// <summary> Allows discord users to register their account for kinklink
    /// </summary>
    public async Task<RegistrationResponse> RegisterUserAsync(ulong discordId)
    {
        // TODO: Check if the discord ID is registered.
        // If not registered, create a new secret key and UID pair
        throw new NotImplementedException("TODO");
    }

    /// <summary> Allows discord users to register their account for kinklink
    /// </summary>
    public async Task<RegistrationResponse> RemoveAccount(ulong discordId)
    {
        // TODO: Check if the discord ID is registered.
        // If not registered, create a new secret key and UID pair
        throw new NotImplementedException("TODO");
    }

    /// <summary> Allows discord users to register their account for kinklink
    /// </summary>
    public async Task<RegistrationResponse> CreateSecondaryUID(ulong discordId)
    {
        // TODO: Check if the discord ID is registered.
        // If Registered, allow them to create a secondary UID
        // If not registered, create a new secret key and UID pair
        throw new NotImplementedException("TODO");
    }
}
