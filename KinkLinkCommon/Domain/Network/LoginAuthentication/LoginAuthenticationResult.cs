using KinkLinkCommon.Domain.Enums;

namespace KinkLinkCommon.Domain.Network.LoginAuthentication;

public record LoginAuthenticationResult(
    LoginAuthenticationErrorCode ErrorCode,
    string? Secret
);
