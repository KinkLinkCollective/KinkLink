using System;
using KinkLinkClient.Dependencies.Glamourer.Domain;

namespace KinkLinkClient.Domain;

public record WardrobeSet(
    Guid Id,
    string Name,
    string Description,
    Design GlamourerDesign
)
{
    public static WardrobeSet FromDesign(Design design, string? name = null, string? description = null)
    {
        return new WardrobeSet(
            Guid.NewGuid(),
            name ?? design.Name,
            description ?? string.Empty,
            design
        );
    }
}
