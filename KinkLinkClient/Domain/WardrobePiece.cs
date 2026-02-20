using System;
using KinkLinkClient.Domain.Dependencies.Glamourer;
using KinkLinkClient.Domain.Dependencies.Glamourer.Components;

namespace KinkLinkClient.Domain;

public record WardrobePiece(
    Guid Id,
    string Name,
    string Description,
    GlamourerEquipmentSlot Slot,
    GlamourerItem Item,
    uint? Dye1,
    uint? Dye2
)
{
    public static WardrobePiece CreateNew(GlamourerEquipmentSlot slot)
    {
        return new WardrobePiece(
            Guid.NewGuid(),
            $"New {slot} Piece",
            string.Empty,
            slot,
            new GlamourerItem
            {
                Apply = true,
                ApplyCrest = false,
                ApplyStain = true,
                Crest = false,
                ItemId = 0,
                Stain = 0,
                Stain2 = 0
            },
            null,
            null
        );
    }
}
