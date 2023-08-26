using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AddressableAutoAddress
{
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
        
        [SerializeField, HideInInspector]
        internal bool GeneratePathScriptFolder;
        
        [SerializeField, HideInInspector]
        internal bool GeneratePathScriptFolderFiles;

        string IAutoAddressSetting.GeneratePath => AssetDatabase.GetAssetPath(ScriptGenerateFolder);
        bool IAutoAddressSetting.GeneratePathFolder => GeneratePathScriptFolder;
        bool IAutoAddressSetting.GeneratePathFolderFiles => GeneratePathScriptFolderFiles;
    }
}