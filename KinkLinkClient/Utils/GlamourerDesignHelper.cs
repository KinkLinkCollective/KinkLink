using System;
using System.Globalization;
using KinkLinkClient.Domain.Dependencies.Glamourer;
using Newtonsoft.Json.Linq;

namespace KinkLinkClient.Utils;

public static class GlamourerDesignHelper
{
    public static GlamourerEquipmentSlot ToEquipmentSlot(string key)
    {
        try
        {
            var parsed = uint.Parse(key, NumberStyles.HexNumber);
            var index = (byte)(parsed >> 16) & 0xFF;
            return (GlamourerEquipmentSlot)(1 << index);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[GlamourerDesignHelper.FromJObject] Unexpected error, {e}");
            return (GlamourerEquipmentSlot)0;
        }
    }

    public static JObject ToJObject(GlamourerDesign design)
    {
        try
        {
            // Convert to a JToken
            var json = JToken.FromObject(design);

            // Creates a link object with two empty arrays
            json["Links"] = new JObject { ["Before"] = new JArray(), ["After"] = new JArray() };

            // Return the object as a JObject
            return json as JObject ?? new JObject();
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[GlamourerDesignHelper.FromJObject] Unexpected error, {e}");
            return new JObject();
        }
    }

    public static GlamourerDesign? FromJObject(JObject? design)
    {
        try
        {
            // Reject null objects
            if (design is null)
                return null;

            // Copy
            var copy = design.DeepClone();

            // Remove Mods & Links
            copy["Links"]?.Parent?.Remove();

            // Create a new domain object
            return copy.ToObject<GlamourerDesign>();
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[GlamourerDesignHelper.FromJObject] Unexpected error, {e}");
            return null;
        }
    }
}
