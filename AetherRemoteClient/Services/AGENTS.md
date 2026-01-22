# CLIENT SERVICES

**Parent:** `AetherRemoteClient/`

## OVERVIEW

Application services layer wrapping core functionality. All services are registered in `Plugin.cs` via DI.

## SERVICES

| Service | Purpose | Dependencies |
|---------|---------|--------------|
| `ActionQueueService` | Queues and throttles actions | - |
| `ChatService` | Chat message handling | - |
| `CommandLockoutService` | Prevents command spam | - |
| `ConfigurationService` | Loads/saves config files | - |
| `EmoteService` | Emote execution | - |
| `FriendsListService` | Friends list sync | - |
| `IdentityService` | Local player identity | - |
| `LogService` | Structured logging | - |
| `NetworkService` | SignalR connection | - |
| `PauseService` | Plugin pause state | - |
| `PermanentTransformationLockService` | Transformation locks | - |
| `TipService` | Tooltip messages | - |
| `ViewService` | Current UI view state | - |
| `WorldService` | World/zone data | - |

## PATTERNS

- **Constructor Injection**: All dependencies passed via constructor
- **Async/Await**: Network I/O is async
- **Singleton**: One instance per plugin session

## NOTES

- `ConfigurationService` handles three config types: main, character (`Characters/`), profiles (`Hypnosis/`)
- `NetworkService` manages SignalR connection lifecycle
- `ViewService` drives MainWindow view switching
