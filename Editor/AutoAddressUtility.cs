using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Properties;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace AddressableAutoAddress
{
    internal static class AutoAddressUtility
    {
        internal static IAutoAddressSetting LoadPostProcessSetting()
        {
            var setting = LoadSetting();
            return setting == null ? new DefaultSetting() : setting;
        }

        internal static AutoAddressSetting LoadSetting()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(AutoAddressSetting)}");
            if (guids.Length <= 0) return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var asset = AssetDatabase.LoadAssetAtPath<AutoAddressSetting>(path);
            return asset;
        }


        /// <summary>
        /// スクリプトを生成する
        /// </summary>
        internal static void GenerateScript(IAutoAddressSetting autoAddressSetting)
        {
            if (!autoAddressSetting.GeneratePathScript || string.IsNullOrEmpty(autoAddressSetting.GeneratePath))
                return;
            
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Please Create Addressables Settings");
                return;
            }

            var generateRootPath = autoAddressSetting.GeneratePath;

            var fileText = GenerateText(autoAddressSetting);
            
            // テキストをファイルに出力する
            var generatePath = string.Format($"{generateRootPath}/AutoAddressPath.cs");
            File.WriteAllText(generatePath, fileText);
        }

        private static readonly string FileNamePattern = @"[\w]+";
        
        private static string GenerateText(IAutoAddressSetting autoSettings)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            
            var builder = new StringBuilder();
            var indent = 0;
            AppendIndent(indent, "using UnityEngine;");
            AppendIndent(indent, "using UnityEngine.AddressableAssets;");
            AppendIndent(indent, "using UnityEngine.ResourceManagement.AsyncOperations;");
            
            AppendIndent(indent, "public static class AutoAddressPath");
            AppendIndent(indent++, "{");

            var lineBuilder = new StringBuilder();
            var generateHashSet = new HashSet<string>();
            var directoryHashSet = new Dictionary<string, string>();
            var directoryFileNames = new Dictionary<string, List<string>>();

            foreach (var group in settings.groups)
            {
                // Builtinは無視
                if (group.HasSchema<PlayerDataGroupSchema>())
                    continue;

                foreach (var entry in group.entries)
                {
                    lineBuilder.Clear();
                    var splits = entry.address.Split("/");
                    var length = splits.Length;
                    for (var i = 0; i < length - 1; i++) 
                        lineBuilder.Append(splits[i]);

                    var path = AssetDatabase.GUIDToAssetPath(entry.guid);
                    var last = splits[length - 1];
                    var isFolder = AssetDatabase.IsValidFolder(path);
                    
                    lineBuilder.Append(
                        !isFolder && length > 1
                            ? $"_{last.Replace(".", "_")}"
                            : last
                    );

                    var fieldName = lineBuilder.ToString();
                    var matches = Regex.Matches(fieldName, FileNamePattern);
                    var regexFileName = string.Join("", matches.Cast<Match>().Select(match => match.Value));

                    // すでに生成しているパスと同じだった場合はしない
                    if (generateHashSet.Contains(regexFileName))
                        continue;
                    
                    AppendIndent(indent, $"// {path}");
                    AppendIndent(indent, $"public static string {regexFileName} => \"{entry.address}\";");    
                    if (isFolder)
                        continue;

                    var index = regexFileName.IndexOf("_", StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        var parentPath = regexFileName.Substring(0, index);
                        if (!string.IsNullOrEmpty(parentPath))
                            directoryHashSet.TryAdd(parentPath, Path.GetDirectoryName(entry.address));

                        if (!directoryFileNames.ContainsKey(parentPath))
                            directoryFileNames.Add(parentPath, new List<string>());
                        
                        directoryFileNames[parentPath].Add(Path.GetFileName(entry.address));
                    }

                    AppendIndent(indent,
                        $"public static AsyncOperationHandle<{entry.MainAssetType}> {regexFileName}Handle => " +
                        $"Addressables.LoadAssetAsync<{entry.MainAssetType}>({regexFileName});");
                    AppendIndent(indent, "");
                }
            }

            if (directoryHashSet.Count > 0)
            {
                AppendIndent(indent, $"// Directories Path");
                foreach (var pair in directoryHashSet)
                {
                    if (autoSettings.GeneratePathFolder)
                    {
                        AppendIndent(indent, $"public static string Dir{pair.Key} => \"{pair.Value}/\";");
                    }

                    if (autoSettings.GeneratePathFolderFiles &&
                        directoryFileNames.TryGetValue(pair.Key, out var values))
                    {
                        AppendIndent(indent++, $"public static string[] Dir{pair.Key}Files => new string[] {{");
                        foreach (var value in values.OrderBy(v => v))
                        {
                            AppendIndent(indent, $"\"{value}\",");
                        }
                        
                        AppendIndent(--indent, "};");
                    }
                }
            }

            AppendIndent(--indent, "}");

            void AppendIndent(int indent, string text)
            {
                for (var i = 0; i < indent; i++)
                    builder.Append("\t");

                builder.AppendLine(text);
            }

            return builder.ToString();
        }

        internal static void OnGUIFolder(SerializedProperty property)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                property.objectReferenceValue = (DefaultAsset) EditorGUILayout.ObjectField(
                    property.name,
                    property.objectReferenceValue,
                    typeof(DefaultAsset),
                    false
                );
                if (check.changed)
                {
                    if (property.objectReferenceValue != null)
                    {
                        var path = AssetDatabase.GetAssetPath(property.objectReferenceValue);
                        // フォルダじゃなければ生成できないので null にする
                        if (!AssetDatabase.IsValidFolder(path))
                        {
                            property.objectReferenceValue = null;
                        }
                    }
                }
            }   
        }
    }
}