using KinkLinkServer.Services;

namespace KinkLinkServer.Domain.Interfaces;

public interface IPresenceService
{
    public Presence? TryGet(string friendCode);

    public void Add(string friendCode, Presence presence);

    public void Remove(string friendCode);

    public bool IsUserExceedingCooldown(string friendCode);

    public int GetOnlineCount();
}
