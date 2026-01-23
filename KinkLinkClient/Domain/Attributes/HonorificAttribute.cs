using System.Threading.Tasks;
using KinkLinkClient.Dependencies.Honorific.Services;
using KinkLinkClient.Domain.Interfaces;
using KinkLinkClient.Utils;
using KinkLinkCommon.Dependencies.Honorific.Domain;

namespace KinkLinkClient.Domain.Attributes;

// ReSharper disable RedundantBoolCompare

public class HonorificAttribute(HonorificService honorific, ushort characterIndex) : ICharacterAttribute
{
    private HonorificInfo _honorific = new();

    public async Task<bool> Store()
    {
        if (await honorific.GetCharacterTitle(characterIndex).ConfigureAwait(false) is not { } json)
        {
            Plugin.Log.Warning("[HonorificAttribute.Store] Could not get character's title");
            return false;
        }

        _honorific = json;
        return true;
    }

    public async Task<bool> Apply(PermanentTransformationData data)
    {
        if (await honorific.SetCharacterTitle(0, _honorific) is false)
        {
            Plugin.Log.Warning("[HonorificAttribute.Apply] Could not set title");
            return false;
        }

        NotificationHelper.Honorific();

        // TODO: Update PermanentTransformationData with Honorific
        return true;
    }
}
