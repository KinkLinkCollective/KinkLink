using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KinkLinkClient;
using KinkLinkClient.Domain.Configurations;
using KinkLinkClient.Domain.Dependencies.Glamourer;
using KinkLinkClient.Domain.Dependencies.Glamourer.Components;

public enum LayerLocks
{
    InnerGag,
    Gag,
    OuterGag,
    Blindfold,
    Collar,
    Head,
    Chest,
    Hands,
    Feet,
    Legs,
    Earring,
    Necklace,
    Wrists,
    RingR,
    RingL,
    Bonus,
    SetLayer,
}

public class RestraintItem
{
    public Guid Id;
    public string Name = string.Empty;
    public string Description = string.Empty;
    public GlamourerEquipmentSlot Slot;
    public GlamourerItem? Item;
    public uint? Dye1;
    public uint? Dye2;
    public Dictionary<string, GlamourerMaterial> Materials = [];
}

// The wardrobe service contains the information about how our character should
// look at dispatches commands to the glamourer API to ensure that we are set how we should be set.
public class WardrobeService
{
    // Do we need these special layer restraints?
    private RestraintItem? InnerGag,
        Gag,
        OuterGag,
        Blindfold,
        Collar;

    // Individual layers
    private RestraintItem? Head,
        Chest,
        Hands,
        Feet,
        Legs,
        Earring,
        Necklace,
        Wrists,
        RingR,
        RingL,
        Bonus;
    private GlamourerBonus? BonusItem;

    // Outfit layer
    private GlamourerDesign? SetLayer;
    private Dictionary<string, RestraintItem> AvailableRestraints;
    private Dictionary<string, GlamourerDesign> AvailableSets;

    public WardrobeService()
    {
        if (Plugin.CharacterConfiguration is { } config)
        {
            AvailableSets = config.WardrobeSets;
            AvailableRestraints = config.WardrobeItems;
        }
        else
        {
            AvailableSets = new();
            AvailableRestraints = new();
        }
    }

    public async Task AddRestraintSet(string name, RestraintItem item)
    {
        AvailableRestraints.Add(name, item);
        await Save();
    }

    public async Task AddAvailableRestraintSet(string name, GlamourerDesign design)
    {
        AvailableSets.Add(name, design);
        await Save();
    }

    private async Task Save()
    {
        if (Plugin.CharacterConfiguration is not null)
        {
            Plugin.CharacterConfiguration.WardrobeSets = AvailableSets;
            Plugin.CharacterConfiguration.WardrobeItems = AvailableRestraints;
            await Plugin.CharacterConfiguration.Save();
        }
    }
    // TODO: Add in the stub functions for wardrobe management.
    // Need to be able to add to the available sets as well as apply sets/items to the various layers
    // Sets are easy, they only affect the layer, but the Restraints will need to be applied by slot.
    // Finally, need to have a handler that is registered to a callback in here so that when the state is finalized it is restored
    // Locks should be applied via a locking service and checked using it.
}
