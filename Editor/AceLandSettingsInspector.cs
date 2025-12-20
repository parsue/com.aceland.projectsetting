using UnityEditor;

namespace AceLand.ProjectSetting.Editor
{
    public abstract class AceLandSettingsInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Content is protected. Please go to Project Settings window to modify values.",
                MessageType.Warning,
                true);
        }
    }
}