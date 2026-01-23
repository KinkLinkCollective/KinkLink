using KinkLinkClient.Domain.Enums;
using Newtonsoft.Json.Linq;

namespace KinkLinkClient.Domain;

public class ApplyGenericTransformationResult(ApplyGenericTransformationErrorCode success, JObject? glamourerJObject)
{
    public readonly ApplyGenericTransformationErrorCode Success = success;
    public readonly JObject? GlamourerJObject = glamourerJObject;
}
