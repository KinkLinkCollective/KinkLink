using MessagePack;

namespace KinkLinkCommon.Domain;

/// <summary>
///     An empty object
/// </summary>
[MessagePackObject(true)]
public readonly struct Unit
{
    public static readonly Unit Empty = new();
}
