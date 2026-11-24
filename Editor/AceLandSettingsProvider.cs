using UnityEditor;
using UnityEngine.UIElements;

namespace AceLand.ProjectSetting.Editor
{
    public abstract class AceLandSettingsProvider : SettingsProvider
    {
        protected internal AceLandSettingsProvider(string path, SettingsScope scope = SettingsScope.User) 
            : base(path, scope) { }
        
        protected SerializedObject Settings;
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.ClearUndo(Settings.targetObject);
        }

        public override void OnGUI(string searchContext)
        {
            Settings.Update();
            
            using var change = new EditorGUI.ChangeCheckScope();
            Undo.RecordObject(Settings.targetObject, "Apply Changes");
                
            ProjectSettingEditorHelper.DrawAllProperties(Settings);
            EditorGUILayout.Space(20f);
        
            if (change.changed)
            {
                Settings.ApplyModifiedProperties();
                OnChanged(Settings);
                EditorUtility.SetDirty(Settings.targetObject);
            }
            else
            {
                Settings.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        protected virtual void OnChanged(SerializedObject settings) { }

        protected virtual void OnUndoRedoPerformed()
        {
            // Refresh the serialized object when undo/redo is performed
            Settings.Update();
            // Optionally, you can also force a repaint if needed
            EditorUtility.SetDirty(Settings.targetObject);
        }
    }
}