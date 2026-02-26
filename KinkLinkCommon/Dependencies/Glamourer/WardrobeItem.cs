namespace KinkLinkCommon.Dependencies.Glamourer;

public class WardrobeItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint ItemId { get; set; }
    public Dictionary<string, object> Data { get; set; } = [];
}
