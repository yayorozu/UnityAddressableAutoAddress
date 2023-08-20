using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AddressableAutoAddress
{
    internal interface IAutoAddressSetting
    {
        string TargetPass { get; }
        
        bool IncludeExtension { get; }
        
        List<string> FolderRegex { get; }
        
        bool GeneratePathScript { get; }
        
        string GeneratePath { get; }
    }
    
    internal class AutoAddressSetting : ScriptableObject, IAutoAddressSetting
    {
        string IAutoAddressSetting.TargetPass => ObserveTargetFolder == null ? "" : AssetDatabase.GetAssetPath(ObserveTargetFolder);

        [SerializeField, HideInInspector]
        internal DefaultAsset ObserveTargetFolder;
        
        bool IAutoAddressSetting.IncludeExtension => IncludeExtension;
        [SerializeField, HideInInspector]
        internal bool IncludeExtension;

        /// <summary>
        /// アドレスをつけるフォルダ
        /// </summary>
        List<string> IAutoAddressSetting.FolderRegex => FolderRegex;
        [SerializeField, HideInInspector]
        internal List<string> FolderRegex;

        bool IAutoAddressSetting.GeneratePathScript => GeneratePathScript;
        
        [SerializeField, HideInInspector]
        internal bool GeneratePathScript;

        [SerializeField, HideInInspector]
        internal DefaultAsset ScriptGenerateFolder;

        string IAutoAddressSetting.GeneratePath => AssetDatabase.GetAssetPath(ScriptGenerateFolder);
    }

    internal class DefaultSetting : IAutoAddressSetting
    {
        public string TargetPass => "Assets/AddressableAsset/";
        public bool IncludeExtension => false;
        public List<string> FolderRegex => null;
        public bool GeneratePathScript => false;
        
        public string GeneratePath => "";
    }

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