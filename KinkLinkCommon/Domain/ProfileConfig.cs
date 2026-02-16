using MessagePack;

namespace KinkLinkCommon.Domain;

[MessagePackObject(keyAsPropertyName: true)]
public record KinkLinkProfile(
    string Uid,
    string? ChatRole,
    string? Alias,
    string? Title,
    string? Description,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);

[MessagePackObject(keyAsPropertyName: true)]
public record KinkLinkProfileConfig(
    bool EnableGlamours,
    bool EnableGarbler,
    bool EnableGarblerChannels,
    bool EnableMoodles
);
