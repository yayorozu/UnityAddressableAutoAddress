using System.Collections.Generic;

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
}