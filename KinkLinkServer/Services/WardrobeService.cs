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
                    ? DeserializeToDict<GlamourerItem>(item)
                    : null,
                null,
                row.Data.TryGetProperty("mods", out var mods)
                    ? DeserializeToDictList(mods)
                    : null,
                row.Data.TryGetProperty("materials", out var materials)
                    ? DeserializeToDict<GlamourerMaterial>(materials)
                    : null,
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

        return new WardrobeDto(
            row.Value.Id,
            row.Value.Name ?? string.Empty,
            row.Value.Description ?? string.Empty,
            row.Value.Type ?? string.Empty,
            (GlamourerEquipmentSlot)(row.Value.Slot ?? 0),
            row.Value.Data.TryGetProperty("item", out var item)
                ? DeserializeToDict<GlamourerItem>(item)
                : null,
            null,
            row.Value.Data.TryGetProperty("mods", out var mods)
                ? DeserializeToDictList(mods)
                : null,
            row.Value.Data.TryGetProperty("materials", out var materials)
                ? DeserializeToDict<GlamourerMaterial>(materials)
                : null,
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

        var data = new
        {
            item = dto.Item,
            mods = dto.Mods,
            materials = dto.Materials,
        };

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
            state.CharacterItems?.Count ?? 0
        );

        var result = await _wardrobeSql.UpdateWardrobeStateAsync(
            new(
                profileId,
                SerializeToJsonElement(state.BaseLayer),
                SerializeToJsonElement(state.Equipment?["Head"]),
                SerializeToJsonElement(state.Equipment?["Body"]),
                SerializeToJsonElement(state.Equipment?["Hands"]),
                SerializeToJsonElement(state.Equipment?["Legs"]),
                SerializeToJsonElement(state.Equipment?["Feet"]),
                SerializeToJsonElement(state.Equipment?["Ears"]),
                SerializeToJsonElement(state.Equipment?["Neck"]),
                SerializeToJsonElement(state.Equipment?["Wrists"]),
                SerializeToJsonElement(state.Equipment?["LFinger"]),
                SerializeToJsonElement(state.Equipment?["RFinger"]),
                SerializeToJsonElement(state.CharacterItems?.Values)
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

        var equipment = new Dictionary<string, object?>();
        var characterItems = new Dictionary<string, object?>();

        if (row.Value.Head.HasValue)
            equipment["Head"] = SerializeToJsonElement(DeserializeNullable<WardrobeItem>(row.Value.Head.Value));
        if (row.Value.Body.HasValue)
            equipment["Body"] = SerializeToJsonElement(DeserializeNullable<WardrobeItem>(row.Value.Body.Value));
        if (row.Value.Hand.HasValue)
            equipment["Hands"] = SerializeToJsonElement(DeserializeNullable<WardrobeItem>(row.Value.Hand.Value));
        if (row.Value.Legs.HasValue)
            equipment["Legs"] = SerializeToJsonElement(DeserializeNullable<WardrobeItem>(row.Value.Legs.Value));
        if (row.Value.Feet.HasValue)
            equipment["Feet"] = SerializeToJsonElement(DeserializeNullable<WardrobeItem>(row.Value.Feet.Value));
        if (row.Value.Earring.HasValue)
            equipment["Ears"] = SerializeToJsonElement(DeserializeNullable<WardrobeItem>(row.Value.Earring.Value));
        if (row.Value.Neck.HasValue)
            equipment["Neck"] = SerializeToJsonElement(DeserializeNullable<WardrobeItem>(row.Value.Neck.Value));
        if (row.Value.Bracelet.HasValue)
            equipment["Wrists"] = SerializeToJsonElement(DeserializeNullable<WardrobeItem>(row.Value.Bracelet.Value));
        if (row.Value.Lring.HasValue)
            equipment["LFinger"] = SerializeToJsonElement(DeserializeNullable<WardrobeItem>(row.Value.Lring.Value));
        if (row.Value.Rring.HasValue)
            equipment["RFinger"] = SerializeToJsonElement(DeserializeNullable<WardrobeItem>(row.Value.Rring.Value));

        if (row.Value.Moditems.HasValue)
        {
            var modItems = DeserializeList<WardrobeItem>(row.Value.Moditems.Value);
            if (modItems != null)
            {
                foreach (var item in modItems)
                {
                    if (item != null)
                        characterItems[item.Id.ToString()] = SerializeToJsonElement(item);
                }
            }
        }

        return new WardrobeStateDto(
            row.Value.Glamourerset.HasValue
                ? ObjectToDict(DeserializeNullable<GlamourerDesign>(row.Value.Glamourerset.Value))
                : null,
            equipment,
            characterItems
        );
    }

    private static Dictionary<string, object?>? ObjectToDict<T>(T? obj) where T : class
    {
        if (obj == null) return null;
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
            if (obj == null) return null;
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.Serialize(obj));
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
}
