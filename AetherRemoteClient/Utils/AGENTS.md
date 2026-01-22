# CLIENT UTILITIES

**Parent:** `AetherRemoteClient/`

## OVERVIEW

Cross-cutting utilities heavily referenced across UI and services.

## UTILITIES (7 files)

| File | Purpose | Refs |
|------|---------|------|
| `SharedUserInterfaces.cs` | ImGui framework: fonts, controls, tooltips, content boxes, icon buttons | 219+ |
| `NotificationHelper.cs` | Standardized success/error/warning notifications | 50+ |
| `GlamourerDesignHelper.cs` | Glamourer JSON manipulation helpers | - |
| `HypnosisSpiralGenerator.cs` | Generates spiral parameters | - |
| `HypnosisTextGenerator.cs` | Generates hypnosis text effects | - |
| `AetherRemoteStyle.cs` | Plugin-specific styling | - |
| `SharedUserInterfaces.cs` | Core ImGui UI helpers | - |

## KEY: SharedUserInterfaces

Most critical file - provides:
- `PushTextFont()`, `PopTextFont()` - Font management
- `ContentBox()` - Standardized content containers
- `IconButton()` - Icon buttons with tooltips
- `CheckboxWithDescription()` - Labeled checkboxes
- Color utilities and layout helpers

## PATTERNS

- **Static Methods**: All utilities are static classes
- **ImGui Chaining**: Uses ImGui's immediate mode pattern
- **No State**: Utilities don't maintain internal state

## ANTI-PATTERNS

- `SharedUserInterfaces.cs` is 339 lines - needs refactoring
- Multiple responsibilities (fonts, colors, components)
