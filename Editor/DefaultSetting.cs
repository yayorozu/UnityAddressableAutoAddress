using System.Collections.Generic;

namespace AddressableAutoAddress
{
    internal class DefaultSetting : IAutoAddressSetting
    {
        public string TargetPass => "Assets/AddressableAsset/";
        public bool IncludeExtension => false;
        public List<string> FolderRegex => null;
        public bool GeneratePathScript => false;
        
        public string GeneratePath => "";
        public bool GeneratePathFolder => false;
        public bool GeneratePathFolderFiles => false;
    }
}