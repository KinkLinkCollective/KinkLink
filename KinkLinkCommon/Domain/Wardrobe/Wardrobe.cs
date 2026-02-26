using KinkLinkCommon.Dependencies.Glamourer;
using KinkLinkCommon.Dependencies.Glamourer.Components;
using KinkLinkCommon.Domain.Enums;

namespace KinkLinkCommon.Domain.Wardrobe;

public record WardrobeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type;
    public GlamourerEquipmentSlot Slot { get; set; }
    public GlamourerItem? Item { get; set; }
    public GlamourerDesign Design { get; set; }
    public List<GlamourerMod> Mods { get; set; } = [];
    public Dictionary<string, GlamourerMaterial> Materials { get; set; } = [];
    public RelationshipPriority Priority { get; set; } = RelationshipPriority.Casual;
}

public record WardrobeStateDto
{
    public GlamourerDesign? _baseLayer;
    public Dictionary<GlamourerEquipmentSlot, WardrobeItem?> _equipment = new();
    public Dictionary<Guid, WardrobeItem> _characterItems = new();
}
