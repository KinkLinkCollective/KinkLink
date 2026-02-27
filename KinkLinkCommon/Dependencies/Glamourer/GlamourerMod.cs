using System.Collections.Generic;
using KinkLinkCommon.Dependencies.Penumbra.Domain;
using MessagePack;

namespace KinkLinkCommon.Dependencies.Glamourer;

[MessagePackObject]
public readonly record struct GlamourerMod(
    [property: Key(0)] string Name,
    [property: Key(1)] string DirectoryName,
    [property: Key(2)] Dictionary<string, List<string>> Settings,
    [property: Key(3)] int Priority,
    [property: Key(4)] bool Enabled,
    [property: Key(5)] bool ForceInherit = false,
    [property: Key(6)] bool Remove = false
)
{
    public (Mod, ModSettings) ToTuple()
    {
        return (
            new Mod(Name, DirectoryName),
            new ModSettings(Settings, Priority, Enabled, ForceInherit, Remove)
        );
    }
};
