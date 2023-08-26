using UnityEditor;
using UnityEngine;

namespace AddressableAutoAddress
{
    [CustomEditor(typeof(AutoAddressSetting))]
    internal class AutoAddressSettingEditor : Editor
    {
        private SerializedProperty ObserveTargetFolder;
        private SerializedProperty IncludeExtension;
        private SerializedProperty FolderRegex;
        private SerializedProperty GeneratePathScript;
        private SerializedProperty ScriptGenerateFolder;
        private SerializedProperty GeneratePathScriptFolder;
        private SerializedProperty GeneratePathScriptFolderFiles;
        
        private void OnEnable()
        {
            ObserveTargetFolder = serializedObject.FindProperty(nameof(AutoAddressSetting.ObserveTargetFolder));
            IncludeExtension = serializedObject.FindProperty(nameof(AutoAddressSetting.IncludeExtension));
            FolderRegex = serializedObject.FindProperty(nameof(AutoAddressSetting.FolderRegex));
            GeneratePathScript = serializedObject.FindProperty(nameof(AutoAddressSetting.GeneratePathScript));
            ScriptGenerateFolder = serializedObject.FindProperty(nameof(AutoAddressSetting.ScriptGenerateFolder));
            GeneratePathScriptFolder = serializedObject.FindProperty(nameof(AutoAddressSetting.GeneratePathScriptFolder));
            GeneratePathScriptFolderFiles = serializedObject.FindProperty(nameof(AutoAddressSetting.GeneratePathScriptFolderFiles));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                serializedObject.UpdateIfRequiredOrScript();
                AutoAddressUtility.OnGUIFolder(ObserveTargetFolder);
                if (ObserveTargetFolder.objectReferenceValue != null)
                {
                    using (new EditorGUI.IndentLevelScope())
                    using (new EditorGUI.DisabledScope(true))
                    {
                        var path = AssetDatabase.GetAssetPath(ObserveTargetFolder.objectReferenceValue);
                        EditorGUILayout.LabelField(path);
                    }
                }

                using (new EditorGUI.DisabledScope(ObserveTargetFolder.objectReferenceValue == null))
                {
                    EditorGUILayout.PropertyField(IncludeExtension);
                    EditorGUILayout.PropertyField(FolderRegex, true);
                    EditorGUILayout.PropertyField(GeneratePathScript);
                    if (GeneratePathScript.boolValue)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            AutoAddressUtility.OnGUIFolder(ScriptGenerateFolder);
                            EditorGUILayout.PropertyField(GeneratePathScriptFolder, new GUIContent("Folder Path"));
                            EditorGUILayout.PropertyField(GeneratePathScriptFolderFiles, new GUIContent("Folder Files Path"));
                        }
                    }
                }
                if (check.changed)
                    serializedObject.ApplyModifiedProperties();
            }
        }
    }
}