using KinkLinkCommon.Domain.Enums.Permissions;
using KinkLinkCommon.Domain.Network;

namespace KinkLinkServer.Domain;


public record SpeakRequestInfo(string Method, GarblerChannels Permissions, ActionCommand Request);
public record PrimaryRequestInfo(string Method, PrimaryPermissions2 Permissions, ActionCommand Request);
