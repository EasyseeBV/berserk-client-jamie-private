using UnityEditor;

namespace RR.Core.Editor.Tools
{
    internal static class ToggleablePreprocessorSymbolsTool
    {
        public const string DebugSymbol = "RR_DEBUG";
        public const string AssetBusSymbol = "RR_ASSETBUS";

        [MenuItem(EditorUtils.ComName + "/Toggle " + DebugSymbol + " " +
#if RR_DEBUG
                  "(✔)"
#else
                  "(✘)"
#endif
        )]
        private static void ToggleDebugSymbol()
        {
#if RR_DEBUG
            SymbolsUtility.RemoveDefineSymbols(s => s == DebugSymbol);
#else
            SymbolsUtility.AddDefineSymbols(DebugSymbol);
#endif
        }  
        
        [MenuItem(EditorUtils.ComName + "/Toggle " + AssetBusSymbol + " " +
#if RR_ASSETBUS
                  "(✔)"
#else
                  "(✘)"
#endif
        )]
        private static void ToggleAssetBusSymbol()
        {
#if RR_DEBUG
            SymbolsUtility.RemoveDefineSymbols(s => s == AssetBusSymbol);
#else
            SymbolsUtility.AddDefineSymbols(AssetBusSymbol);
#endif
        }
    }
}