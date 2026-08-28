# Build Levels

`BuildLevel` is a small enum shared across AceLand packages. It gives you a single, consistent way to say
"run this only in the Editor", "…in development builds" or "…in production" — without scattering
`#if UNITY_EDITOR` / `#if DEBUG` all over your code.

## The Enum
```csharp
public enum BuildLevel
{
    None,        // never accepted
    Editor,      // Editor only
    Development, // development builds (and above)
    Production,  // production builds (and above)
}
```

Think of `Editor → Development → Production` as an ascending ladder. The current build sits on exactly one
rung, and the helpers below compare a chosen level against that current rung.

## The Current Level
`BuildLevelUtils.CurrentLevel` tells you where the running build sits:

| Compilation context | `CurrentLevel` |
| --- | --- |
| In the Unity Editor | `BuildLevel.Editor` |
| Development build (`DEBUG`) | `BuildLevel.Development` |
| Release / production build | `BuildLevel.Production` |

```csharp
using AceLand.ProjectSetting;

Debug.Log($"Running as: {BuildLevelUtils.CurrentLevel}");
```

---

## Helper Methods
The `BuildLevelExtensions` methods answer "should this run right now?" so you don't have to write the
preprocessor checks yourself. `None` always returns `false`.

| Method | Returns true when… | Typical use |
| --- | --- | --- |
| `IsAcceptedLevel()` | current build is **at or above** the chosen level | enable a feature from a level upward |
| `IsAcceptedLevelInvert()` | current build is **at or below** the chosen level | enable a feature up to a level |
| `IsAcceptedLevelOnly()` | current build **exactly matches** the chosen level | Editor-only / dev-only diagnostics |

### `IsAcceptedLevel()` — "from this level up"
The most common one. A `loggingLevel` of `Development` means: log in Development **and** Production builds,
but treat Editor as included too (Editor is the lowest accepted rung).

```csharp
[SerializeField] private BuildLevel loggingLevel = BuildLevel.Development;

public void Log(string message)
{
    // In Editor: accepted. In dev build: accepted. In release: accepted.
    // Set to BuildLevel.Production to silence Editor + dev builds.
    if (loggingLevel.IsAcceptedLevel())
        Debug.Log(message);
}
```

{% hint style="info" %}
Reading the table can feel backwards at first. The rule is simple: the **field value is the threshold**,
and the check passes when the **current build** meets that threshold for the chosen direction.
{% endhint %}

### `IsAcceptedLevelOnly()` — "only on this rung"
Use it for diagnostics that must never leak past a specific build type.

```csharp
[SerializeField] private BuildLevel verboseTrace = BuildLevel.Editor;

if (verboseTrace.IsAcceptedLevelOnly())
    Debug.Log("This trace shows in the Editor only, never in any build.");
```

### `IsAcceptedLevelInvert()` — "up to this level"
Handy for things you want in early builds but not in the final release.

```csharp
[SerializeField] private BuildLevel cheatMenu = BuildLevel.Development;

// Shown in Editor and development builds; hidden in production.
if (cheatMenu.IsAcceptedLevelInvert())
    ShowCheatMenu();
```

---

## Using It in a Settings Type
Because `BuildLevel` lives in the same package as `ProjectSettings<T>`, it drops straight into your
settings asset and shows up as a normal dropdown in the settings window.

```csharp
using AceLand.ProjectSetting;
using UnityEngine;

public class MyGameSettings : ProjectSettings<MyGameSettings>
{
    [SerializeField] private BuildLevel loggingLevel = BuildLevel.Development;

    public bool PrintLogging => loggingLevel.IsAcceptedLevel();
}
```

---

## Best Practices
- Reach for `BuildLevel` instead of raw `#if` blocks whenever a toggle is really "which build type" — it
  keeps the decision data-driven and editable from the settings window.
- Default logging fields to `Development` so the Editor stays talkative while production stays quiet.
- Use `IsAcceptedLevelOnly()` for anything that must never appear outside one specific build type.
- Remember `None` is the explicit "off" switch — it fails every check.
