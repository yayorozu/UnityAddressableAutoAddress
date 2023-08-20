using System;
using System.IO;
using System.Text.RegularExpressions;
using AddressableAutoAddress;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AddressablePostProcessor : AssetPostprocessor
{
    private enum Mode
    {
        Add,
        Delete,
        Move
    }

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var setting = AutoAddressUtility.LoadPostProcessSetting();
        if (string.IsNullOrEmpty(setting.GeneratePath))
            return;
        
        var dirty = false;
        dirty |= CheckPath(setting, importedAssets, Mode.Add);
        dirty |= CheckPath(setting, deletedAssets, Mode.Delete);
        dirty |= CheckPath(setting, movedAssets, Mode.Move);
        
        if (dirty)
        {
            AutoAddressUtility.GenerateScript(setting);
            
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }

    private static bool CheckPath(IAutoAddressSetting autoSettings, string[] paths, Mode mode)
    {
        var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
        if (addressableSettings == null)
        {
            Debug.LogError("Please Create Addressables Settings");
            return false;
        }
        var groups = addressableSettings.groups;
        var dirty = false;

        var targetPass = $"{autoSettings.TargetPass}/"; 
       
        foreach (var path in paths)
        {
            if (!path.StartsWith(targetPass))
                continue;
            
            var assetPath = path.Replace(targetPass, "");

            var targetGroup = addressableSettings.DefaultGroup;
            var rootIndex = assetPath.IndexOf("/", StringComparison.Ordinal);
            // 流石にもうひとつディレクトリほしい
            if (rootIndex < 0)
                continue;

            var rootPath = assetPath.Substring(0, rootIndex);
            var groupIndex = groups.FindIndex(g => g.Name == rootPath);
            if (groupIndex >= 0)
            {
                targetGroup = groups[groupIndex];
            }
            else
            {
                var groupTemplate = addressableSettings.GetGroupTemplateObject(0) as AddressableAssetGroupTemplate;
                targetGroup =
                    addressableSettings.CreateGroup(rootPath, false, false, false, null, groupTemplate.GetTypes());
            }

            assetPath = assetPath.Substring(rootIndex + 1);

            var applyMode = CanApplyAddress(path) ? mode : Mode.Delete;
            var guid = AssetDatabase.AssetPathToGUID(path);
            dirty |= ChangeEntry(targetGroup, assetPath, guid, applyMode);
        }

        return dirty;

        bool CanApplyAddress(string path)
        {
            var isFolder = AssetDatabase.IsValidFolder(path);
            // Regex 登録がない場合はフォルダーじゃなければ名前をつける
            if (autoSettings.FolderRegex.Count > 0)
            {
                var dirName = isFolder ? path : Path.GetDirectoryName(path); 
                
                foreach (var baseRegex in autoSettings.FolderRegex)
                {
                    if (!Regex.IsMatch(dirName, baseRegex))
                        continue;

                    //Debug.Log($"{path} {dirName}");

                    // フォルダの場合親フォルダが引っかかったらそれで終了にしたいので確認する
                    if (isFolder && IsParentMatch(dirName, baseRegex))
                        continue;
                    
                    // ヒットした
                    return isFolder;
                }
            }

            return !isFolder;
        }

        // 親フォルダがマッチしているかどうか
        bool IsParentMatch(string dirName, string regex)
        {
            var parent = Path.GetDirectoryName(dirName);
            while (parent != autoSettings.TargetPass)
            {
                if (Regex.IsMatch(parent, regex))
                    return true;
                
                parent = Path.GetDirectoryName(parent);
            }

            return false;
        }
        
        bool ChangeEntry(
            AddressableAssetGroup group,
            string path,
            string guid,
            Mode mode
        )
        {
            if (mode == Mode.Delete)
            {
                var findEntry = addressableSettings.FindAssetEntry(guid) != null; 
                if (findEntry)
                {
                    addressableSettings.RemoveAssetEntry(guid);
                }

                return findEntry;
            }
                
            if (mode == Mode.Add || mode == Mode.Move)
            {
                var address = path;
                // 拡張子をつけるかつけないか
                if (!autoSettings.IncludeExtension)
                {
                    var lastIndex = path.LastIndexOf(".", StringComparison.Ordinal);
                    if (lastIndex >= 1)
                    {
                        address = path.Substring(0, lastIndex);
                    }
                }
                
                var entry = addressableSettings.CreateOrMoveEntry(guid, group);
                if (entry.address == address)
                    return false;
                    
                entry.address = address;
            }
            return true;
        }
    }

}
