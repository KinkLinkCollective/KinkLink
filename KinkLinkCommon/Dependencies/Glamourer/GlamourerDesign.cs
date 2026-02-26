using KinkLinkCommon.Dependencies.Glamourer.Components;

namespace KinkLinkCommon.Dependencies.Glamourer;

public class GlamourerDesign
{
    public GlamourerCustomize? Customize { get; set; }
    public GlamourerEquipment? Equipment { get; set; }
    public Dictionary<string, GlamourerItem> Component { get; set; } = [];
}

public class GlamourerCustomize
{
    public byte[] Data { get; set; } = [];
}

public class GlamourerEquipment
{
    public GlamourerItem Head { get; set; } = new();
    public GlamourerItem Body { get; set; } = new();
    public GlamourerItem Hands { get; set; } = new();
    public GlamourerItem Legs { get; set; } = new();
    public GlamourerItem Feet { get; set; } = new();
    public GlamourerItem Ears { get; set; } = new();
    public GlamourerItem Neck { get; set; } = new();
    public GlamourerItem Wrists { get; set; } = new();
    public GlamourerItem RFinger { get; set; } = new();
    public GlamourerItem LFinger { get; set; } = new();
}
