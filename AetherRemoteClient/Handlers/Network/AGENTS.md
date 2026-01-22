# CLIENT NETWORK HANDLERS

**Parent:** `AetherRemoteClient/`

## OVERVIEW

Client-side request handlers that send actions to server. Mirror server-side `SignalR/Handlers/`.

## HANDLERS (12 files)

| Handler | Server Handler | Purpose |
|---------|----------------|---------|
| `BodySwapHandler` | `BodySwapHandler` | Swap bodies with targets |
| `CustomizePlusHandler` | `CustomizePlusHandler` | Apply Customize+ profiles |
| `EmoteHandler` | `EmoteHandler` | Force emote on targets |
| `HonorificHandler` | `HonorificHandler` | Apply honorific titles |
| `HypnosisHandler` | `HypnosisHandler` | Send hypnosis profile |
| `HypnosisStopHandler` | `HypnosisStopHandler` | Stop target hypnosis |
| `MoodlesHandler` | `MoodlesHandler` | Apply Moodle effects |
| `SpeakHandler` | `SpeakHandler` | Send chat messages |
| `SyncOnlineStatusHandler` | `SyncOnlineStatusHandler` | Sync online state |
| `SyncPermissionsHandler` | `SyncPermissionsHandler` | Sync permission matrix |
| `TransformHandler` | `TransformHandler` | Apply character transform |
| `TwinningHandler` | `TwinningHandler` | Clone player appearance |

## PATTERNS

- **Base Class**: All extend `BaseHandler` (error handling, validation)
- **Network Call**: Each calls `NetworkService.SendAsync()`
- **Result Handling**: Returns `ActionResponse` with error code
- **Mare Disabled**: TwinningHandler, TransformHandler, BodySwapHandler have `TODO: Re-enable when a mare solution is found`

## ADDING NEW HANDLER

1. Create handler in `Handlers/Network/`
2. Add server counterpart in `AetherRemoteServer/SignalR/Handlers/`
3. Register both in DI (`Plugin.cs`, `Program.cs`)
4. Add network DTOs in `AetherRemoteCommon/Domain/Network/`
