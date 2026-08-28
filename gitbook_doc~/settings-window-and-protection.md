# Settings Window & Protection

Once a type derives from `ProjectSettings<T>`, you get two Editor-side surfaces to wire up: a
**Project Settings window** where users actually edit the values, and an **Inspector lock** that stops the
asset being edited by accident. This page covers both, plus the helper that draws properties for you.

## The Asset Lifecycle
`ProjectSettings<T>` never exposes a constructor. Instead, the Editor asks for the asset through a single
static entry point:

```csharp
// Returns a SerializedObject wrapping the single asset for this type.
SerializedObject settings = MyGameSettings.GetSerializedSettings();
```

Behind that call the base class loads `Assets/Resources/<TypeName>.asset`, force-imports it if it exists on
disk but isn't registered yet, and only creates a fresh default when it truly doesn't exist. That ordering
is what protects a teammate's checked-in settings from being reset on a first pull — see
[How It Works](README.md#how-it-works).

{% hint style="warning" %}
`GetSerializedSettings()` is compiled under `UNITY_EDITOR` only. Never call it from runtime code — read the
asset with `Resources.Load<T>(nameof(T))` instead.
{% endhint %}

---

## The Project Settings Window
Derive from `AceLandSettingsProvider` to place your settings under **Edit ▸ Project Settings**. The base
class already wires up undo/redo, change tracking and dirtying, so a minimal provider is short.

```csharp
using AceLand.ProjectSetting.Editor;
using UnityEditor;
using UnityEngine.UIElements;

public class MyGameSettingsProvider : AceLandSettingsProvider
{
    public const string SETTINGS_NAME = "Project/My Game";

    private MyGameSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
        : base(path, scope) { }

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        base.OnActivate(searchContext, rootElement);
        // Load the asset into the base class' Settings field.
        Settings = MyGameSettings.GetSerializedSettings();
    }

    [SettingsProvider]
    public static SettingsProvider Create()
        => new MyGameSettingsProvider(SETTINGS_NAME, SettingsScope.Project);
}
```

### What the base `OnGUI` does for you
If you don't override `OnGUI`, the base implementation:

1. Calls `Settings.Update()` to pull the latest serialized state.
2. Opens a change-check scope and records an undo step.
3. Draws **every visible property** via `ProjectSettingEditorHelper.DrawAllProperties`.
4. On change, applies modified properties and marks the target dirty; otherwise applies without undo.

That is enough for most settings. Provide your own `OnGUI` only when you need a custom layout, grouping or
buttons — the [Web Request settings provider](README.md) is an example of a fully custom layout.

### Drawing helpers
`ProjectSettingEditorHelper` exposes two static helpers you can reuse in a custom `OnGUI`:

| Method | Behaviour |
| --- | --- |
| `DrawAllProperties(settings, includeScript = false)` | Iterates every visible property and draws it. Skips the `m_Script` field unless `includeScript` is true. |
| `DrawAllPropertiesAsDisabled(settings, includeScript = true)` | Same iteration, wrapped in a disabled group — useful for a read-only preview. |

```csharp
public override void OnGUI(string searchContext)
{
    Settings.Update();

    EditorGUILayout.LabelField("My Game", EditorStyles.boldLabel);
    ProjectSettingEditorHelper.DrawAllProperties(Settings);

    Settings.ApplyModifiedProperties();
}
```

---

## Locking the Asset in the Inspector
The asset lives in `Resources`, so it's visible and clickable in the Project window. To stop users editing
it there — and to steer them to the settings window — add a custom Editor that derives from
`AceLandSettingsInspector`:

```csharp
using AceLand.ProjectSetting.Editor;
using UnityEditor;

[CustomEditor(typeof(MyGameSettings))]
public class MyGameSettingsInspector : AceLandSettingsInspector { }
```

The Inspector then replaces the editable fields with a protected notice:

{% hint style="info" %}
*Content is protected. Please go to Project Settings window to modify values.*
{% endhint %}

This is purely a UI guard — it prevents accidental edits from the Inspector. The values themselves are
still plain serialized fields, so the settings window (and code) can read and write them normally.

---

## Putting It Together
A complete, protected settings type is three small pieces:

{% tabs %}
{% tab title="1. Settings (Runtime)" %}
```csharp
using AceLand.ProjectSetting;
using UnityEngine;

public class MyGameSettings : ProjectSettings<MyGameSettings>
{
    [SerializeField] private string gameName = "My Game";
    public string GameName => gameName;
}
```
{% endtab %}

{% tab title="2. Provider (Editor)" %}
```csharp
using AceLand.ProjectSetting.Editor;
using UnityEditor;
using UnityEngine.UIElements;

public class MyGameSettingsProvider : AceLandSettingsProvider
{
    private MyGameSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
        : base(path, scope) { }

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        base.OnActivate(searchContext, rootElement);
        Settings = MyGameSettings.GetSerializedSettings();
    }

    [SettingsProvider]
    public static SettingsProvider Create()
        => new MyGameSettingsProvider("Project/My Game", SettingsScope.Project);
}
```
{% endtab %}

{% tab title="3. Inspector lock (Editor)" %}
```csharp
using AceLand.ProjectSetting.Editor;
using UnityEditor;

[CustomEditor(typeof(MyGameSettings))]
public class MyGameSettingsInspector : AceLandSettingsInspector { }
```
{% endtab %}
{% endtabs %}

---

## Best Practices
- Keep the provider thin — let the base `OnGUI` and `ProjectSettingEditorHelper` do the drawing unless you
  truly need a custom layout.
- Always pair a settings type with an `AceLandSettingsInspector` subclass so the asset can't be edited from
  the Inspector.
- Load the asset only through `GetSerializedSettings()` in Editor code; use `Resources.Load` at runtime.
- Give each provider a clear `Project/...` path so related packages group together in the settings window.
