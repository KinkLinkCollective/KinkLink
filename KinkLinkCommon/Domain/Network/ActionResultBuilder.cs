using KinkLinkCommon.Domain.Enums;

namespace KinkLinkCommon.Domain.Network;

public static class ActionResultBuilder
{
    public static ActionResult<Unit> Ok() => new(ActionResultEc.Success, Unit.Empty);
    public static ActionResult<T> Ok<T>(T value) => new(ActionResultEc.Success, value);
    public static ActionResult<Unit> Fail(ActionResultEc error) => new(error, Unit.Empty);
    public static ActionResult<T> Fail<T>(ActionResultEc error) => new(error, Activator.CreateInstance<T>());
}
