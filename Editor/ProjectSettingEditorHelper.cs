using UnityEditor;

namespace AceLand.ProjectSetting.Editor
{
    public static class ProjectSettingEditorHelper
    {
        public static void DrawAllProperties(SerializedObject settings, bool includeScript = false)
        {
            var iterator = settings.GetIterator();
            iterator.NextVisible(true);
            
            do
            {
                var name = iterator.name;
                if (!includeScript && name == "m_Script") continue;
                EditorGUILayout.PropertyField(settings.FindProperty(name));
            } while (iterator.NextVisible(false));
            
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
        
        public static void DrawAllPropertiesAsDisabled(SerializedObject settings, bool includeScript = true)
        {
            EditorGUI.BeginDisabledGroup(true);
            DrawAllProperties(settings, includeScript);
            EditorGUI.EndDisabledGroup();
        }
    }
}