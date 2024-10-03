using RR.Core.DebugSystem;
using RR.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace RR.Core.Editor.Tools
{
	public static class SymbolsUtility
    {
        public static void AddDefineSymbols(params string[] symbols)
        {
            Enum.GetValues(typeof(BuildTarget))
                .ToList<BuildTarget>()
                .ForEach(target => AddDefineSymbols(target, symbols));
        }

        public static void AddDefineSymbols(BuildTarget target, params string[] symbols)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            if (targetGroup == BuildTargetGroup.Unknown)
                return; // TODO: Check if it is correct

            var definedSymbols = GetDefinedSymbols(targetGroup).ToList();
            var newDefineSymbols = definedSymbols.Concat(symbols).Distinct(StringComparer.Ordinal).ToList();

            if (definedSymbols.Count == newDefineSymbols.Count)
                return;

            try
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    targetGroup,
                    string.Join(";", newDefineSymbols)
                );
            }
            catch (Exception e)
            {
                RRLogger.Error($"Could not set {string.Join(",", symbols)} defines for build target: {target} group: {targetGroup} {e}");
            }
        }

        public static void RemoveDefineSymbols(BuildTarget target, IEnumerable<string> symbols)
        {
            RemoveDefineSymbols(target, symbols.Contains);
        }

        public static void RemoveDefineSymbols(params string[] symbols)
        {
            RemoveDefineSymbols(symbols.Contains);
        }

        public static void RemoveDefineSymbols(Func<string, bool> symbolPredicate)
        {
            Enum.GetValues(typeof(BuildTarget))
                .ToList<BuildTarget>()
                .ForEach(target => RemoveDefineSymbols(target, symbolPredicate));
        }

        public static void RemoveDefineSymbols(BuildTarget target, Func<string, bool> symbolPredicate)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            if (targetGroup == BuildTargetGroup.Unknown)
                return; // TODO: Check if it is correct

            var activeSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';');
            var symbolsToKeep = activeSymbols.Select(symbol => symbol.Trim()).Where(symbol => !symbolPredicate(symbol)).ToList();

            if (symbolsToKeep.Count == activeSymbols.Length)
                return;

            try
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", symbolsToKeep));
            }
            catch (Exception e)
            {
				RRLogger.Error($"Could not set {string.Join(",", symbolsToKeep)} defines for build target: {target} group: {targetGroup} {e}");
            }
        }

        public static IEnumerable<string> GetDefinedSymbols(BuildTarget target)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            return GetDefinedSymbols(targetGroup);
        }

        public static IEnumerable<string> GetDefinedSymbols(BuildTargetGroup targetGroup)
        {
            return PlayerSettings
                .GetScriptingDefineSymbolsForGroup(targetGroup)
                .Split(';')
                .Select(symbol => symbol.Trim())
                .Where(symbol => !string.IsNullOrEmpty(symbol));
        }
    }
}
