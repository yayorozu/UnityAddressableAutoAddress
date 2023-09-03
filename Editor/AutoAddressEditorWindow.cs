using System;
using UnityEditor;
using UnityEngine;

namespace AddressableAutoAddress
{
    internal class AutoAddressEditorWindow : EditorWindow
    {
        [MenuItem("Tools/AddressableAutoAddress")]
        private static void ShowWindow()
        {
            var window = GetWindow<AutoAddressEditorWindow>();
            window.titleContent = new GUIContent("AutoAddress");
            window.Show();
        }

        private AutoAddressSetting _setting;
        private Editor _editor;
        
        private void Load()
        {
            _setting = AutoAddressUtility.LoadSetting();
        }

        private void OnEnable()
        {
            Load();
        }

        private void OnDisable()
        {
            _setting = null;
        }

        private void OnGUI()
        {
            if (_setting == null)
            {
                if (GUILayout.Button("Create Setting File"))
                {
                    var path = EditorUtility.SaveFilePanelInProject(
                        "Select File Save Location",
                        $"{nameof(AutoAddressSetting)}",
                        "asset", 
                        "Please select where to save the file and click 'Save'.");
                    if (!string.IsNullOrEmpty(path))
                    {
                        var setting = CreateInstance<AutoAddressSetting>();
                        AssetDatabase.CreateAsset(setting, path);
                        AssetDatabase.SaveAssets();
                        
                        Load();
                    }
                }
                return;
            }

            if (_editor == null)
            {
                _editor = Editor.CreateEditor(_setting);
            }
            
            _editor.OnInspectorGUI();

            if (GUILayout.Button("Generate Script"))
            {
                var setting = AutoAddressUtility.LoadPostProcessSetting();
                AutoAddressUtility.GenerateScript(setting);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
    }
}