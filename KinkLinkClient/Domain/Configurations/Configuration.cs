using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KinkLinkClient.Managers;
using KinkLinkClient.Services;

namespace KinkLinkClient.Domain.Configurations;

/// <summary>
///     The global configuration
/// </summary>
[Serializable]
public class Configuration
{
    /// <summary>
    ///     Configuration version
    /// </summary>
    public int Version = 1;

    /// <summary>
    ///     Is the plugin in safe mode
    /// </summary>
    public bool SafeMode = false;

    /// <summary>
    ///     Map of friend code to note
    /// </summary>
    public Dictionary<string, string> Notes = [];

    /// <summary>
    ///     Save the configuration
    /// </summary>
    public async Task Save() => await ConfigurationService.SaveConfiguration(this).ConfigureAwait(false);
}
