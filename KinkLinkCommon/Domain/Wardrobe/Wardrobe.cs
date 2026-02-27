using KinkLinkCommon.Dependencies.Glamourer;
using KinkLinkCommon.Dependencies.Glamourer.Components;
using KinkLinkCommon.Domain.Enums;
using MessagePack;

namespace KinkLinkCommon.Domain.Wardrobe;

[MessagePackObject]
public record WardrobeDto(
    [property: Key(0)] Guid Id,
    [property: Key(1)] string Name,
    [property: Key(2)] string Description,
    [property: Key(3)] string Type,
    [property: Key(4)] GlamourerEquipmentSlot Slot,
    // Item serialized to an base64 string
    [property: Key(5)] GlamourerItem? Item,
    // Item serialized to an base64 string
    [property: Key(6)] GlamourerDesign? Design,
    // Item serialized to an base64 string
    [property: Key(7)] List<GlamourerMod>? Mods,
    // Materials serialized to a base64 string
    [property: Key(8)] Dictionary<string, GlamourerMaterial> Materials,
    [property: Key(9)] RelationshipPriority Priority
);

[MessagePackObject]
public record WardrobeItemData(
    [property: Key(0)] Guid Id,
    [property: Key(1)] string Name,
    [property: Key(2)] string Description,
    [property: Key(3)] GlamourerEquipmentSlot Slot,
    [property: Key(4)] GlamourerItem? Item,
    [property: Key(5)] List<GlamourerMod>? Mods,
    [property: Key(6)] Dictionary<string, GlamourerMaterial>? Materials,
    [property: Key(7)] RelationshipPriority Priority
);

[MessagePackObject]
public record WardrobeStateDto(
    // Serialized as a base64 string
    [property: Key(0)] GlamourerDesign? BaseLayer,
    // Slot name to WardrobeItemData mapping
    [property: Key(1)] Dictionary<string, WardrobeItemData>? Equipment,
    // Mod name to WardrobeItemData mapping
    [property: Key(2)] Dictionary<string, WardrobeItemData>? ModSettings
);
