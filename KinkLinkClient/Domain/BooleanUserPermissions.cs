using KinkLinkCommon.Domain;
using KinkLinkCommon.Domain.Enums.Permissions;

namespace KinkLinkClient.Domain;

// ReSharper disable ConvertIfStatementToReturnStatement

/// <summary>
///     Class representing the <see cref="UserPermissions"/> object but as booleans for use with ImGui
/// </summary>
public class BooleanUserPermissions
{
    // Primary Permissions
    public bool Emote, Customization, Equipment, Mods, BodySwap, Twinning, CustomizePlus, Moodles, Hypnosis, Honorific;

    // Speak Permissions
    public bool Say, Yell, Shout, Tell, Party, Alliance, FreeCompany, PvPTeam, Echo, Roleplay;

    // Linkshell Permissions
    public bool Ls1, Ls2, Ls3, Ls4, Ls5, Ls6, Ls7, Ls8, Cwl1, Cwl2, Cwl3, Cwl4, Cwl5, Cwl6, Cwl7, Cwl8;

    // Elevated Permissions
    public bool PermanentTransformation;

    /// <summary>
    ///     Tests if this object is equal to another <see cref="BooleanUserPermissions"/>
    /// </summary>
    public bool Equals(BooleanUserPermissions other)
    {
        // Primary Permissions
        if (Emote != other.Emote) return false;
        if (Customization != other.Customization) return false;
        if (Equipment != other.Equipment) return false;
        if (Mods != other.Mods) return false;
        if (BodySwap != other.BodySwap) return false;
        if (Twinning != other.Twinning) return false;
        if (CustomizePlus != other.CustomizePlus) return false;
        if (Moodles != other.Moodles) return false;
        if (Hypnosis != other.Hypnosis) return false;
        if (Honorific != other.Honorific) return false;

        // Speak Permissions
        if (Say != other.Say) return false;
        if (Yell != other.Yell) return false;
        if (Shout != other.Shout) return false;
        if (Tell != other.Tell) return false;
        if (Party != other.Party) return false;
        if (Alliance != other.Alliance) return false;
        if (FreeCompany != other.FreeCompany) return false;
        if (PvPTeam != other.PvPTeam) return false;
        if (Echo != other.Echo) return false;
        if (Roleplay != other.Roleplay) return false;

        // Linkshell Permissions
        if (Ls1 != other.Ls1) return false;
        if (Ls2 != other.Ls2) return false;
        if (Ls3 != other.Ls3) return false;
        if (Ls4 != other.Ls4) return false;
        if (Ls5 != other.Ls5) return false;
        if (Ls6 != other.Ls6) return false;
        if (Ls7 != other.Ls7) return false;
        if (Ls8 != other.Ls8) return false;
        if (Cwl1 != other.Cwl1) return false;
        if (Cwl2 != other.Cwl2) return false;
        if (Cwl3 != other.Cwl3) return false;
        if (Cwl4 != other.Cwl4) return false;
        if (Cwl5 != other.Cwl5) return false;
        if (Cwl6 != other.Cwl6) return false;
        if (Cwl7 != other.Cwl7) return false;
        if (Cwl8 != other.Cwl8) return false;

        // Elevated Permissions
        if (PermanentTransformation != other.PermanentTransformation) return false;

        return true;
    }

    /// <summary>
    ///     Converts a <see cref="UserPermissions"/> into a <see cref="BooleanUserPermissions"/>
    /// </summary>
    public static BooleanUserPermissions From(UserPermissions permissions)
    {
        return new BooleanUserPermissions
        {
            // Primary
        };
    }

    /// <summary>
    ///     Converts a <see cref="BooleanUserPermissions"/> to <see cref="UserPermissions"/>
    /// </summary>
    public static UserPermissions To(BooleanUserPermissions permissions)
    {
        // Initialization
        var primary = PrimaryPermissions2.None;
        var speak = SpeakPermissions2.None;
        var elevated = ElevatedPermissions.None;

        // Primary
        if (permissions.Emote) primary |= PrimaryPermissions2.Emote;
        if (permissions.Customization) primary |= PrimaryPermissions2.GlamourerCustomization;
        if (permissions.Equipment) primary |= PrimaryPermissions2.GlamourerEquipment;
        if (permissions.Mods) primary |= PrimaryPermissions2.Mods;
        if (permissions.BodySwap) primary |= PrimaryPermissions2.BodySwap;
        if (permissions.Twinning) primary |= PrimaryPermissions2.Twinning;
        if (permissions.CustomizePlus) primary |= PrimaryPermissions2.CustomizePlus;
        if (permissions.Moodles) primary |= PrimaryPermissions2.Moodles;
        if (permissions.Hypnosis) primary |= PrimaryPermissions2.Hypnosis;
        if (permissions.Honorific) primary |= PrimaryPermissions2.Honorific;

        // Speak Permissions
        if (permissions.Say) speak |= SpeakPermissions2.Say;
        if (permissions.Yell) speak |= SpeakPermissions2.Yell;
        if (permissions.Shout) speak |= SpeakPermissions2.Shout;
        if (permissions.Tell) speak |= SpeakPermissions2.Tell;
        if (permissions.Party) speak |= SpeakPermissions2.Party;
        if (permissions.Alliance) speak |= SpeakPermissions2.Alliance;
        if (permissions.FreeCompany) speak |= SpeakPermissions2.FreeCompany;
        if (permissions.PvPTeam) speak |= SpeakPermissions2.PvPTeam;
        if (permissions.Echo) speak |= SpeakPermissions2.Echo;
        if (permissions.Roleplay) speak |= SpeakPermissions2.Roleplay;

        // Linkshell Permissions
        if (permissions.Ls1) speak |= SpeakPermissions2.Ls1;
        if (permissions.Ls2) speak |= SpeakPermissions2.Ls2;
        if (permissions.Ls3) speak |= SpeakPermissions2.Ls3;
        if (permissions.Ls4) speak |= SpeakPermissions2.Ls4;
        if (permissions.Ls5) speak |= SpeakPermissions2.Ls5;
        if (permissions.Ls6) speak |= SpeakPermissions2.Ls6;
        if (permissions.Ls7) speak |= SpeakPermissions2.Ls7;
        if (permissions.Ls8) speak |= SpeakPermissions2.Ls8;
        if (permissions.Cwl1) speak |= SpeakPermissions2.Cwl1;
        if (permissions.Cwl2) speak |= SpeakPermissions2.Cwl2;
        if (permissions.Cwl3) speak |= SpeakPermissions2.Cwl3;
        if (permissions.Cwl4) speak |= SpeakPermissions2.Cwl4;
        if (permissions.Cwl5) speak |= SpeakPermissions2.Cwl5;
        if (permissions.Cwl6) speak |= SpeakPermissions2.Cwl6;
        if (permissions.Cwl7) speak |= SpeakPermissions2.Cwl7;
        if (permissions.Cwl8) speak |= SpeakPermissions2.Cwl8;

        // Elevated Permissions
        if (permissions.PermanentTransformation) elevated |= ElevatedPermissions.PermanentTransformation;

        return new UserPermissions();
    }
}
