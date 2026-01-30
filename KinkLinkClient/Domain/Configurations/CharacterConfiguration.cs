using System;
using System.Threading.Tasks;
using KinkLinkClient.Services;

namespace KinkLinkClient.Domain.Configurations;

/// <summary>
///     The individual character configuration file 
/// </summary>
[Serializable]
public class CharacterConfiguration
{
    /// <summary>
    ///     Configuration version
    /// </summary>
    public int Version = 1;

    /// <summary>
    ///     Character's name
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    ///     Character's home world
    /// </summary>
    public string World = string.Empty;

    /// <summary>
    ///     Should the plugin login automatically
    /// </summary>
    public bool AutoLogin;

    /// <summary>
    ///     This is the UID that should be associated with this character for login. 
    ///     This is the last UID that was used by default, but can be changed at the login screen.
    /// </summary>
    public string ProfileUID = string.Empty;

    /// <summary>
    ///     Save the configuration
    /// </summary>
    public async Task Save() => await ConfigurationService.SaveCharacterConfiguration(this).ConfigureAwait(false);
}
