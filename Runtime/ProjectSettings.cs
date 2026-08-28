using System.IO;
using UnityEditor;
using UnityEngine;

namespace AceLand.ProjectSetting
{
    public abstract class ProjectSettings<T> : ScriptableObject
        where T : ScriptableObject
    {
        private const string PATH = "Assets/Resources";

#if UNITY_EDITOR
        private static T GetOrCreateSettings()
        {
            var settingName = typeof(T).Name;
            var fullPath = PATH + "/" + settingName + ".asset";
            var settings = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            if (settings != null) return settings;

            // The asset file may already exist on disk but not yet be registered in the
            // AssetDatabase - this happens on the first project open after pulling from
            // source control. Force-import it and reload before deciding to create a new
            // one, otherwise the user's existing settings would be overwritten by a
            // fresh default asset.
            if (File.Exists(fullPath))
            {
                AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceSynchronousImport);
                settings = AssetDatabase.LoadAssetAtPath<T>(fullPath);
                if (settings != null) return settings;
            }

            if (!Directory.Exists(PATH)) Directory.CreateDirectory(PATH);
            
            settings = CreateInstance<T>();
            AssetDatabase.CreateAsset(settings, fullPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Create Project Settings in {PATH} : {settingName}");
            
            return settings;
        }
        
        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
#endif
    }
}