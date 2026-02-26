namespace KinkLinkCommon.Dependencies.Glamourer.Components;

public class GlamourerMod
{
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];
}
