namespace KinkLinkCommon.Domain.Network.GetToken;

/// <summary>
/// Request to log into the server
/// </summary>
public record GetTokenRequest(
    string Secret,
    string ProfileUID,
    Version Version
);
