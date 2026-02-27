using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KinkLinkClient.Domain.Dependencies.Glamourer;
using KinkLinkClient.Domain.Dependencies.Glamourer.Components;
using KinkLinkClient.Utils;
using KinkLinkCommon.Domain.Enums;
using KinkLinkCommon.Domain.Network;
using KinkLinkCommon.Domain.Wardrobe;
using GlamourerSlot = KinkLinkCommon.Dependencies.Glamourer.GlamourerEquipmentSlot;
using ClientSlot = KinkLinkClient.Domain.Dependencies.Glamourer.GlamourerEquipmentSlot;
using ClientDesign = KinkLinkClient.Domain.Dependencies.Glamourer.GlamourerDesign;

namespace KinkLinkClient.Services;

public class WardrobeNetworkService : IDisposable
{
    private readonly NetworkService _networkService;
    private WardrobeService? _wardrobeService;

    public WardrobeNetworkService(NetworkService networkService)
    {
        _networkService = networkService;
    }

    public void SetWardrobeService(WardrobeService wardrobeService)
    {
        _wardrobeService = wardrobeService;
    }

    public async Task SyncFromServerAsync()
    {
        if (_wardrobeService == null)
        {
            Plugin.Log.Warning("[WardrobeNetworkService] WardrobeService not set, skipping sync");
            return;
        }

        try
        {
            var types = new[] { "item", "set", "moditem" };
            foreach (var type in types)
            {
                var result = await ListWardrobeItemsAsync(type);
                if (result.Result == ActionResultEc.Success && result.Value != null)
                {
                    foreach (var dto in result.Value)
                    {
                        ApplyWardrobeDto(dto);
                    }
                    Plugin.Log.Information("[WardrobeNetworkService] Synced {Count} {Type} items from server", result.Value.Count, type);
                }
            }

            var statusResult = await GetWardrobeStatusAsync();
            if (statusResult.Result == ActionResultEc.Success && statusResult.Value != null)
            {
                ApplyWardrobeState(statusResult.Value);
                Plugin.Log.Information("[WardrobeNetworkService] Synced wardrobe status from server");
            }

            NotificationHelper.Success("Wardrobe Sync", "Synced wardrobe from server");
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "[WardrobeNetworkService] Failed to sync from server");
            NotificationHelper.Error("Wardrobe Sync Failed", "Failed to sync wardrobe from server");
        }
    }

    private void ApplyWardrobeDto(WardrobeDto dto)
    {
        if (_wardrobeService == null)
            return;

        switch (dto.Type)
        {
            case "item":
                var item = DtoToWardrobeItem(dto);
                _wardrobeService.AddPiece(item);
                break;
            case "set":
                var set = DtoToGlamourerDesign(dto);
                _wardrobeService.AddSet(set);
                break;
            case "moditem":
                var modItem = DtoToWardrobeItem(dto);
                _wardrobeService.AddModItem(modItem);
                break;
        }
    }

    private static WardrobeItem DtoToWardrobeItem(WardrobeDto dto)
    {
        var slot = ConvertSlot(dto.Slot);
        return new WardrobeItem
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Slot = slot,
            Priority = dto.Priority
        };
    }

    private static ClientDesign DtoToGlamourerDesign(WardrobeDto dto)
    {
        return new ClientDesign
        {
            Identifier = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Priority = dto.Priority
        };
    }

    private void ApplyWardrobeState(WardrobeStateDto state)
    {
        if (_wardrobeService == null)
            return;

        if (state.BaseLayer != null && state.BaseLayer.TryGetValue("Identifier", out var baseLayerIdObj))
        {
            if (baseLayerIdObj is Guid baseLayerId)
            {
                var set = _wardrobeService.GetSetById(baseLayerId);
                if (set != null)
                {
                    _wardrobeService.ApplySetByIdSync(baseLayerId);
                }
            }
        }

        if (state.Equipment != null)
        {
            foreach (var kvp in state.Equipment)
            {
                if (kvp.Value is Dictionary<string, object?> itemDict && itemDict.TryGetValue("Id", out var itemIdObj))
                {
                    if (itemIdObj is Guid itemId)
                    {
                        var piece = _wardrobeService.GetPieceById(itemId);
                        if (piece != null)
                        {
                            var slot = ConvertSlotKey(kvp.Key);
                            if (slot != ClientSlot.None)
                            {
                                _wardrobeService.ApplyPieceSync(slot, piece);
                            }
                        }
                    }
                }
            }
        }

        if (state.CharacterItems != null)
        {
            foreach (var kvp in state.CharacterItems)
            {
                if (kvp.Value is Guid charItemId)
                {
                    var modItem = _wardrobeService.GetModItemById(charItemId);
                    if (modItem != null)
                    {
                        _wardrobeService.ApplyCharacterItemSync(modItem);
                    }
                }
            }
        }
    }

    private static ClientSlot ConvertSlot(GlamourerSlot slot)
    {
        return slot switch
        {
            GlamourerSlot.Head => ClientSlot.Head,
            GlamourerSlot.Body => ClientSlot.Body,
            GlamourerSlot.Hands => ClientSlot.Hands,
            GlamourerSlot.Legs => ClientSlot.Legs,
            GlamourerSlot.Feet => ClientSlot.Feet,
            GlamourerSlot.Ears => ClientSlot.Ears,
            GlamourerSlot.Neck => ClientSlot.Neck,
            GlamourerSlot.Wrists => ClientSlot.Wrists,
            GlamourerSlot.RFinger => ClientSlot.RFinger,
            GlamourerSlot.LFinger => ClientSlot.LFinger,
            _ => ClientSlot.None,
        };
    }

    private static ClientSlot ConvertSlotKey(string slotName)
    {
        return slotName switch
        {
            "Head" => ClientSlot.Head,
            "Body" => ClientSlot.Body,
            "Hands" => ClientSlot.Hands,
            "Legs" => ClientSlot.Legs,
            "Feet" => ClientSlot.Feet,
            "Ears" => ClientSlot.Ears,
            "Neck" => ClientSlot.Neck,
            "Wrists" => ClientSlot.Wrists,
            "RFinger" => ClientSlot.RFinger,
            "LFinger" => ClientSlot.LFinger,
            _ => ClientSlot.None,
        };
    }

    public async Task<ActionResult<WardrobeDto>> AddWardrobeItemAsync(WardrobeDto request)
    {
        try
        {
            var response = await _networkService
                .InvokeAsync<ActionResult<WardrobeDto>>(HubMethod.AddWardrobeItem, request)
                .ConfigureAwait(false);

            if (response.Result != ActionResultEc.Success)
            {
                NotificationHelper.Error("Add Wardrobe Item", $"Failed to add wardrobe item: {response.Result}");
            }

            return response;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "[WardrobeNetworkService] Failed to add wardrobe item");
            NotificationHelper.Error("Add Wardrobe Item", "Failed to add wardrobe item to server");
            return new ActionResult<WardrobeDto>(ActionResultEc.Unknown, null);
        }
    }

    public async Task<ActionResult<bool>> RemoveWardrobeItemAsync(Guid wardrobeId)
    {
        try
        {
            var response = await _networkService
                .InvokeAsync<ActionResult<bool>>(HubMethod.RemoveWardrobeItem, wardrobeId)
                .ConfigureAwait(false);

            if (response.Result != ActionResultEc.Success)
            {
                NotificationHelper.Error("Remove Wardrobe Item", $"Failed to remove wardrobe item: {response.Result}");
            }

            return response;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "[WardrobeNetworkService] Failed to remove wardrobe item");
            NotificationHelper.Error("Remove Wardrobe Item", "Failed to remove wardrobe item from server");
            return new ActionResult<bool>(ActionResultEc.Unknown, false);
        }
    }

    public async Task<ActionResult<WardrobeDto>> GetWardrobeItemAsync(Guid wardrobeId)
    {
        try
        {
            var response = await _networkService
                .InvokeAsync<ActionResult<WardrobeDto>>(HubMethod.GetWardrobeItem, wardrobeId)
                .ConfigureAwait(false);

            return response;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "[WardrobeNetworkService] Failed to get wardrobe item");
            NotificationHelper.Error("Get Wardrobe Item", "Failed to get wardrobe item from server");
            return new ActionResult<WardrobeDto>(ActionResultEc.Unknown, null);
        }
    }

    public async Task<ActionResult<List<WardrobeDto>>> ListWardrobeItemsAsync(string type)
    {
        try
        {
            var response = await _networkService
                .InvokeAsync<ActionResult<List<WardrobeDto>>>(HubMethod.ListWardrobeItems, type)
                .ConfigureAwait(false);

            return response;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "[WardrobeNetworkService] Failed to list wardrobe items");
            NotificationHelper.Error("List Wardrobe Items", "Failed to list wardrobe items from server");
            return new ActionResult<List<WardrobeDto>>(ActionResultEc.Unknown, []);
        }
    }

    public async Task<ActionResult<bool>> SetWardrobeStatusAsync(WardrobeStateDto state)
    {
        try
        {
            var response = await _networkService
                .InvokeAsync<ActionResult<bool>>(HubMethod.SetWardrobeStatus, state)
                .ConfigureAwait(false);

            if (response.Result != ActionResultEc.Success)
            {
                NotificationHelper.Error("Set Wardrobe Status", $"Failed to set wardrobe status: {response.Result}");
            }

            return response;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "[WardrobeNetworkService] Failed to set wardrobe status");
            NotificationHelper.Error("Set Wardrobe Status", "Failed to set wardrobe status on server");
            return new ActionResult<bool>(ActionResultEc.Unknown, false);
        }
    }

    public async Task<ActionResult<WardrobeStateDto>> GetWardrobeStatusAsync()
    {
        try
        {
            var response = await _networkService
                .InvokeAsync<ActionResult<WardrobeStateDto>>(HubMethod.GetWardrobeStatus)
                .ConfigureAwait(false);

            return response;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "[WardrobeNetworkService] Failed to get wardrobe status");
            NotificationHelper.Error("Get Wardrobe Status", "Failed to get wardrobe status from server");
            return new ActionResult<WardrobeStateDto>(ActionResultEc.Unknown, null);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
