using UnityEditor;

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
        
        private void OnEnable()
        {
            ObserveTargetFolder = serializedObject.FindProperty(nameof(AutoAddressSetting.ObserveTargetFolder));
            IncludeExtension = serializedObject.FindProperty(nameof(AutoAddressSetting.IncludeExtension));
            FolderRegex = serializedObject.FindProperty(nameof(AutoAddressSetting.FolderRegex));
            GeneratePathScript = serializedObject.FindProperty(nameof(AutoAddressSetting.GeneratePathScript));
            ScriptGenerateFolder = serializedObject.FindProperty(nameof(AutoAddressSetting.ScriptGenerateFolder));
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
                        }
                    }
                }
                if (check.changed)
                    serializedObject.ApplyModifiedProperties();
            }
        }
    }
}