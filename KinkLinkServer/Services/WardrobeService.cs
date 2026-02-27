using System.Text.Json;
using KinkLinkCommon.Database;
using KinkLinkCommon.Dependencies.Glamourer;
using KinkLinkCommon.Dependencies.Glamourer.Components;
using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Domain.Wardrobe;
using KinkLinkServer.Domain;

namespace KinkLinkServer.Services;

public class WardrobeDataService
{
    private readonly ILogger<WardrobeDataService> _logger;
    private readonly WardrobeSql _wardrobeSql;

    public WardrobeDataService(Configuration config, ILogger<WardrobeDataService> logger)
    {
        _logger = logger;
        _wardrobeSql = new WardrobeSql(config.DatabaseConnectionString);
    }

    public async Task<List<WardrobeDto>> GetAllWardrobeByTypeAsync(int profileId, string type)
    {
        _logger.LogDebug(
            "GetAllWardrobeByTypeAsync called with profileId: {ProfileId}, type: {Type}",
            profileId,
            type
        );

        var rows = await _wardrobeSql.GetAllWardrobeByTypeAsync(new(profileId, type));

        _logger.LogDebug(
            "GetAllWardrobeByTypeAsync returned {Count} rows for profileId: {ProfileId}, type: {Type}",
            rows.Count,
            profileId,
            type
        );
        return rows.Select(row => new WardrobeDto(
                row.Id,
                row.Name ?? string.Empty,
                row.Description ?? string.Empty,
                row.Type ?? string.Empty,
                (GlamourerEquipmentSlot)(row.Slot ?? 0),
                row.Data.TryGetProperty("item", out var item)
                    ? DeserializeGlamourerType<GlamourerItem>(item)
                    : null,
                row.Data.TryGetProperty("design", out var design)
                    ? DeserializeGlamourerType<GlamourerDesign>(design)
                    : null,
                row.Data.TryGetProperty("mods", out var mods)
                    ? DeserializeGlamourerType<List<GlamourerMod>>(mods)
                    : null,
                row.Data.TryGetProperty("materials", out var materials)
                    ? DeserializeGlamourerType<Dictionary<string, GlamourerMaterial>>(materials) ?? new()
                    : new(),
                (RelationshipPriority)(row.RelationshipPriority ?? 0)
            ))
            .ToList();
    }

    public async Task<WardrobeDto?> GetWardrobeItemByGuid(int profileId, Guid wardrobeId)
    {
        _logger.LogDebug(
            "GetWardrobeItemByGuid called with profileId: {ProfileId}, wardrobeId: {WardrobeId}",
            profileId,
            wardrobeId
        );

        var row = await _wardrobeSql.GetWardrobeItemByGuidAsync(new(profileId, wardrobeId));

        if (row == null)
        {
            _logger.LogDebug(
                "GetWardrobeItemByGuid found no item for profileId: {ProfileId}, wardrobeId: {WardrobeId}",
                profileId,
                wardrobeId
            );
            return null;
        }

        _logger.LogDebug(
            "GetWardrobeItemByGuid found item {Name} for profileId: {ProfileId}, wardrobeId: {WardrobeId}",
            row.Value.Name,
            profileId,
            wardrobeId
        );
        var glamourerDesign = row.Value.Data.TryGetProperty("design", out var design)
            ? DeserializeGlamourerType<GlamourerDesign>(design)
            : null;
        var glamourerItem = row.Value.Data.TryGetProperty("item", out var item)
            ? DeserializeGlamourerType<GlamourerItem>(item)
            : null;
        var glamourerMods = row.Value.Data.TryGetProperty("mods", out var mods)
            ? DeserializeGlamourerType<List<GlamourerMod>>(mods)
            : null;
        var glamourerMaterials = row.Value.Data.TryGetProperty("materials", out var materials)
            ? DeserializeGlamourerType<Dictionary<string, GlamourerMaterial>>(materials) ?? new()
            : new();
        return new WardrobeDto(
            row.Value.Id,
            row.Value.Name ?? string.Empty,
            row.Value.Description ?? string.Empty,
            row.Value.Type ?? string.Empty,
            (GlamourerEquipmentSlot)(row.Value.Slot ?? 0),
            glamourerItem,
            glamourerDesign,
            glamourerMods,
            glamourerMaterials,
            (RelationshipPriority)(row.Value.RelationshipPriority ?? 0)
        );
    }

    public async Task<bool> CreateOrUpdateWardrobeItemsByNameAsync(
        int profileId,
        Guid uuid,
        WardrobeDto dto
    )
    {
        _logger.LogInformation(
            "CreateOrUpdateWardrobeItemsByNameAsync called with profileId: {ProfileId}, uuid: {Uuid}, name: {Name}",
            profileId,
            uuid,
            dto.Name
        );

        object data;
        if (dto.Design != null)
        {
            data = new { design = dto.Design };
        }
        else
        {
            data = new
            {
                item = dto.Item,
                mods = dto.Mods,
                materials = dto.Materials,
            };
        }

        var dataJson = JsonSerializer.SerializeToElement(data);

        var result = await _wardrobeSql.CreateOrUpdateWardrobeAsync(
            new(
                uuid,
                profileId,
                dto.Name,
                dto.Type,
                dto.Description,
                (int)dto.Slot,
                (int)dto.Priority,
                dataJson
            )
        );

        if (result != null)
        {
            _logger.LogInformation(
                "CreateOrUpdateWardrobeItemsByNameAsync successfully upserted wardrobe item {Uuid} for profileId: {ProfileId}",
                uuid,
                profileId
            );
        }
        else
        {
            _logger.LogWarning(
                "CreateOrUpdateWardrobeItemsByNameAsync failed to upsert wardrobe item {Uuid} for profileId: {ProfileId}",
                uuid,
                profileId
            );
        }

        return result != null;
    }

    public async Task<bool> DeleteWardrobeItemAsync(int profileId, Guid wardrobeId)
    {
        _logger.LogInformation(
            "DeleteWardrobeItemAsync called with profileId: {ProfileId}, wardrobeId: {WardrobeId}",
            profileId,
            wardrobeId
        );

        var result = await _wardrobeSql.DeleteWardrobeAsync(new(profileId, wardrobeId));

        if (result != null)
        {
            _logger.LogInformation(
                "DeleteWardrobeItemAsync successfully deleted wardrobe item {WardrobeId} for profileId: {ProfileId}",
                wardrobeId,
                profileId
            );
        }
        else
        {
            _logger.LogWarning(
                "DeleteWardrobeItemAsync failed to delete wardrobe item {WardrobeId} for profileId: {ProfileId}",
                wardrobeId,
                profileId
            );
        }

        return result != null;
    }

    public async Task<bool> UpdateWardrobeStateAsync(int profileId, WardrobeStateDto state)
    {
        _logger.LogInformation(
            "UpdateWardrobeStateAsync called with profileId: {ProfileId}, equipment count: {EquipmentCount}, characterItems count: {CharacterItemsCount}",
            profileId,
            state.Equipment?.Count ?? 0,
            state.ModSettings?.Count ?? 0
        );

        WardrobeItemData? head = null, body = null, hands = null, legs = null;
        WardrobeItemData? feet = null, ears = null, neck = null, wrists = null;
        WardrobeItemData? lFinger = null, rFinger = null;
        state.Equipment?.TryGetValue("Head", out head);
        state.Equipment?.TryGetValue("Body", out body);
        state.Equipment?.TryGetValue("Hands", out hands);
        state.Equipment?.TryGetValue("Legs", out legs);
        state.Equipment?.TryGetValue("Feet", out feet);
        state.Equipment?.TryGetValue("Ears", out ears);
        state.Equipment?.TryGetValue("Neck", out neck);
        state.Equipment?.TryGetValue("Wrists", out wrists);
        state.Equipment?.TryGetValue("LFinger", out lFinger);
        state.Equipment?.TryGetValue("RFinger", out rFinger);

        var result = await _wardrobeSql.UpdateWardrobeStateAsync(
            new(
                profileId,
                SerializeToJsonElement(state.BaseLayer),
                SerializeToJsonElement(head),
                SerializeToJsonElement(body),
                SerializeToJsonElement(hands),
                SerializeToJsonElement(legs),
                SerializeToJsonElement(feet),
                SerializeToJsonElement(ears),
                SerializeToJsonElement(neck),
                SerializeToJsonElement(wrists),
                SerializeToJsonElement(lFinger),
                SerializeToJsonElement(rFinger),
                SerializeToJsonElement(state.ModSettings?.Values)
            )
        );

        if (result != null)
        {
            _logger.LogInformation(
                "UpdateWardrobeStateAsync successfully updated wardrobe state for profileId: {ProfileId}",
                profileId
            );
        }
        else
        {
            _logger.LogWarning(
                "UpdateWardrobeStateAsync failed to update wardrobe state for profileId: {ProfileId}",
                profileId
            );
        }

        return result != null;
    }

    public async Task<WardrobeStateDto?> GetWardrobeStateAsync(int profileId)
    {
        _logger.LogDebug("GetWardrobeStateAsync called with profileId: {ProfileId}", profileId);

        var row = await _wardrobeSql.GetWardrobeStateAsync(
            new WardrobeSql.GetWardrobeStateArgs(profileId)
        );

        if (row == null)
        {
            _logger.LogDebug(
                "GetWardrobeStateAsync found no state for profileId: {ProfileId}",
                profileId
            );
            return null;
        }

        _logger.LogDebug("GetWardrobeStateAsync found state for profileId: {ProfileId}", profileId);

        var equipment = new Dictionary<string, WardrobeItemData>();
        var modSettings = new Dictionary<string, WardrobeItemData>();

        if (row.Value.Head.HasValue)
        {
            var item = DeserializeNullable<WardrobeItemData>(row.Value.Head.Value);
            if (item != null)
                equipment["Head"] = item;
        }
        if (row.Value.Body.HasValue)
        {
            var item = DeserializeNullable<WardrobeItemData>(row.Value.Body.Value);
            if (item != null)
                equipment["Body"] = item;
        }
        if (row.Value.Hand.HasValue)
        {
            var item = DeserializeNullable<WardrobeItemData>(row.Value.Hand.Value);
            if (item != null)
                equipment["Hands"] = item;
        }
        if (row.Value.Legs.HasValue)
        {
            var item = DeserializeNullable<WardrobeItemData>(row.Value.Legs.Value);
            if (item != null)
                equipment["Legs"] = item;
        }
        if (row.Value.Feet.HasValue)
        {
            var item = DeserializeNullable<WardrobeItemData>(row.Value.Feet.Value);
            if (item != null)
                equipment["Feet"] = item;
        }
        if (row.Value.Earring.HasValue)
        {
            var item = DeserializeNullable<WardrobeItemData>(row.Value.Earring.Value);
            if (item != null)
                equipment["Ears"] = item;
        }
        if (row.Value.Neck.HasValue)
        {
            var item = DeserializeNullable<WardrobeItemData>(row.Value.Neck.Value);
            if (item != null)
                equipment["Neck"] = item;
        }
        if (row.Value.Bracelet.HasValue)
        {
            var item = DeserializeNullable<WardrobeItemData>(row.Value.Bracelet.Value);
            if (item != null)
                equipment["Wrists"] = item;
        }
        if (row.Value.Lring.HasValue)
        {
            var item = DeserializeNullable<WardrobeItemData>(row.Value.Lring.Value);
            if (item != null)
                equipment["LFinger"] = item;
        }
        if (row.Value.Rring.HasValue)
        {
            var item = DeserializeNullable<WardrobeItemData>(row.Value.Rring.Value);
            if (item != null)
                equipment["RFinger"] = item;
        }

        if (row.Value.Moditems.HasValue)
        {
            var modItems = DeserializeList<WardrobeItemData>(row.Value.Moditems.Value);
            if (modItems != null)
            {
                foreach (var item in modItems)
                {
                    if (item != null)
                    {
                        if (item.Mods != null)
                        {
                            foreach (var mod in item.Mods)
                            {
                                modSettings[mod.Name] = item;
                            }
                        }
                    }
                }
            }
        }

        GlamourerDesign? baseLayer = null;
        if (row.Value.Glamourerset.HasValue)
        {
            baseLayer = DeserializeNullable<GlamourerDesign>(row.Value.Glamourerset.Value);
        }

        return new WardrobeStateDto(
            baseLayer,
            equipment.Count > 0 ? equipment : null,
            modSettings.Count > 0 ? modSettings : null
        );
    }

    private static Dictionary<string, object?>? ObjectToDict<T>(T? obj)
        where T : class
    {
        if (obj == null)
            return null;
        var json = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
    }

    private static JsonElement? SerializeToJsonElement<T>(T? value)
    {
        if (value == null)
            return null;
        return JsonSerializer.SerializeToElement(value);
    }

    private static T? DeserializeNullable<T>(JsonElement element)
        where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(element.GetRawText());
        }
        catch
        {
            return null;
        }
    }

    private static List<T> DeserializeList<T>(JsonElement element)
    {
        try
        {
            return JsonSerializer.Deserialize<List<T>>(element.GetRawText()) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static Dictionary<TKey, TValue> DeserializeDict<TKey, TValue>(JsonElement element)
        where TKey : notnull
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(element.GetRawText()) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static Dictionary<string, object?>? DeserializeToDict<T>(JsonElement element)
        where T : class
    {
        try
        {
            var obj = JsonSerializer.Deserialize<T>(element.GetRawText());
            if (obj == null)
                return null;
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(
                JsonSerializer.Serialize(obj)
            );
        }
        catch
        {
            return null;
        }
    }

    private static List<object?>? DeserializeToDictList(JsonElement element)
    {
        try
        {
            var list = JsonSerializer.Deserialize<List<object>>(element.GetRawText());
            return list?.Cast<object?>().ToList();
        }
        catch
        {
            return null;
        }
    }

    private static T? DeserializeGlamourerType<T>(JsonElement element)
    {
        try
        {
            var json = element.GetRawText();
            return JsonSerializer.Deserialize<T>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
        catch (Exception)
        {
            return default(T);
        }
    }
}
