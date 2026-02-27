using KinkLinkCommon.Dependencies.Glamourer;
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
    [property: Key(5)] Dictionary<string, object?>? Item,
    [property: Key(6)] Dictionary<string, object?>? Design,
    [property: Key(7)] List<object?>? Mods,
    [property: Key(8)] Dictionary<string, object?>? Materials,
    [property: Key(9)] RelationshipPriority Priority
);

[MessagePackObject]
public record WardrobeStateDto(
    [property: Key(0)] Dictionary<string, object?>? BaseLayer,
    [property: Key(1)] Dictionary<string, object?>? Equipment,
    [property: Key(2)] Dictionary<string, object?>? CharacterItems
);
