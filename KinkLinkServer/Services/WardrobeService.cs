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

        return rows.Select(row => new WardrobeDto
            {
                Id = row.Id,
                Name = row.Name ?? string.Empty,
                Description = row.Description ?? string.Empty,
                Slot = (GlamourerEquipmentSlot)(row.Slot ?? 0),
                Item = row.Data.TryGetProperty("item", out var item)
                    ? DeserializeNullable<GlamourerItem>(item)
                    : null,
                Mods = row.Data.TryGetProperty("mods", out var mods)
                    ? DeserializeList<GlamourerMod>(mods)
                    : [],
                Materials = row.Data.TryGetProperty("materials", out var materials)
                    ? DeserializeDict<string, GlamourerMaterial>(materials)
                    : [],
                Priority = (RelationshipPriority)(row.RelationshipPriority ?? 0),
            })
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

        return new WardrobeDto
        {
            Id = row.Value.Id,
            Name = row.Value.Name ?? string.Empty,
            Description = row.Value.Description ?? string.Empty,
            Slot = (GlamourerEquipmentSlot)(row.Value.Slot ?? 0),
            Item = row.Value.Data.TryGetProperty("item", out var item)
                ? DeserializeNullable<GlamourerItem>(item)
                : null,
            Mods = row.Value.Data.TryGetProperty("mods", out var mods)
                ? DeserializeList<GlamourerMod>(mods)
                : [],
            Materials = row.Value.Data.TryGetProperty("materials", out var materials)
                ? DeserializeDict<string, GlamourerMaterial>(materials)
                : [],
            Priority = (RelationshipPriority)(row.Value.RelationshipPriority ?? 0),
        };
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
            state._equipment.Count,
            state._characterItems.Count
        );

        var result = await _wardrobeSql.UpdateWardrobeStateAsync(
            new(
                profileId,
                SerializeToJsonElement(state._baseLayer),
                SerializeToJsonElement(
                    state._equipment.GetValueOrDefault(GlamourerEquipmentSlot.Head)
                ),
                SerializeToJsonElement(
                    state._equipment.GetValueOrDefault(GlamourerEquipmentSlot.Body)
                ),
                SerializeToJsonElement(
                    state._equipment.GetValueOrDefault(GlamourerEquipmentSlot.Hands)
                ),
                SerializeToJsonElement(
                    state._equipment.GetValueOrDefault(GlamourerEquipmentSlot.Legs)
                ),
                SerializeToJsonElement(
                    state._equipment.GetValueOrDefault(GlamourerEquipmentSlot.Feet)
                ),
                SerializeToJsonElement(
                    state._equipment.GetValueOrDefault(GlamourerEquipmentSlot.Ears)
                ),
                SerializeToJsonElement(
                    state._equipment.GetValueOrDefault(GlamourerEquipmentSlot.Neck)
                ),
                SerializeToJsonElement(
                    state._equipment.GetValueOrDefault(GlamourerEquipmentSlot.Wrists)
                ),
                SerializeToJsonElement(
                    state._equipment.GetValueOrDefault(GlamourerEquipmentSlot.LFinger)
                ),
                SerializeToJsonElement(
                    state._equipment.GetValueOrDefault(GlamourerEquipmentSlot.RFinger)
                ),
                SerializeToJsonElement(state._characterItems.Values)
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

        var equipment = new Dictionary<GlamourerEquipmentSlot, WardrobeItem?>();
        var characterItems = new Dictionary<Guid, WardrobeItem>();

        if (row.Value.Head.HasValue)
            equipment[GlamourerEquipmentSlot.Head] = DeserializeNullable<WardrobeItem>(
                row.Value.Head.Value
            );
        if (row.Value.Body.HasValue)
            equipment[GlamourerEquipmentSlot.Body] = DeserializeNullable<WardrobeItem>(
                row.Value.Body.Value
            );
        if (row.Value.Hand.HasValue)
            equipment[GlamourerEquipmentSlot.Hands] = DeserializeNullable<WardrobeItem>(
                row.Value.Hand.Value
            );
        if (row.Value.Legs.HasValue)
            equipment[GlamourerEquipmentSlot.Legs] = DeserializeNullable<WardrobeItem>(
                row.Value.Legs.Value
            );
        if (row.Value.Feet.HasValue)
            equipment[GlamourerEquipmentSlot.Feet] = DeserializeNullable<WardrobeItem>(
                row.Value.Feet.Value
            );
        if (row.Value.Earring.HasValue)
            equipment[GlamourerEquipmentSlot.Ears] = DeserializeNullable<WardrobeItem>(
                row.Value.Earring.Value
            );
        if (row.Value.Neck.HasValue)
            equipment[GlamourerEquipmentSlot.Neck] = DeserializeNullable<WardrobeItem>(
                row.Value.Neck.Value
            );
        if (row.Value.Bracelet.HasValue)
            equipment[GlamourerEquipmentSlot.Wrists] = DeserializeNullable<WardrobeItem>(
                row.Value.Bracelet.Value
            );
        if (row.Value.Lring.HasValue)
            equipment[GlamourerEquipmentSlot.LFinger] = DeserializeNullable<WardrobeItem>(
                row.Value.Lring.Value
            );
        if (row.Value.Rring.HasValue)
            equipment[GlamourerEquipmentSlot.RFinger] = DeserializeNullable<WardrobeItem>(
                row.Value.Rring.Value
            );

        if (row.Value.Moditems.HasValue)
        {
            var modItems = DeserializeList<WardrobeItem>(row.Value.Moditems.Value);
            if (modItems != null)
            {
                foreach (var item in modItems)
                {
                    if (item != null)
                        characterItems[item.Id] = item;
                }
            }
        }

        return new()
        {
            _baseLayer = row.Value.Glamourerset.HasValue
                ? DeserializeNullable<GlamourerDesign>(row.Value.Glamourerset.Value)
                : null,
            _equipment = equipment,
            _characterItems = characterItems,
        };
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
}
