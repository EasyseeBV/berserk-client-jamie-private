using System;
using System.Collections.Generic;
using System.Linq;
using RR.Core.Extensions;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

namespace RR.Core.Editor.Tools
{
	public static class EditorSymbolsUtility
    {
        public static void AddDefineSymbols(params string[] symbols)
        {
            Enum.GetValues(typeof(BuildTarget))
                .ToList<BuildTarget>()
                .Select<BuildTarget, (BuildTarget target, BuildTargetGroup targetGroup)>(target => (target, BuildPipeline.GetBuildTargetGroup(target)))
                .Where(tuple => tuple.targetGroup != BuildTargetGroup.Unknown)
                .ForEach(tuple =>
                {
                    var (target, targetGroup) = tuple;

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
                        Debug.Log($"Could not set {string.Join(",", symbols)} defines for build target: {target} group: {targetGroup} {e}");
                    }
                });
        }

        public static void RemoveDefineSymbols(IEnumerable<string> symbols)
        {
            RemoveDefineSymbols(symbols.Contains);
        }

        public static void RemoveDefineSymbols(Func<string, bool> symbolPredicate)
        {
            Enum.GetValues(typeof(BuildTarget))
				.ToList<BuildTarget>()
				.Select<BuildTarget, (BuildTarget target, BuildTargetGroup targetGroup)>(target => (target, BuildPipeline.GetBuildTargetGroup(target)))
                .Where(tuple => tuple.targetGroup != BuildTargetGroup.Unknown)
                .ForEach(tuple =>
                {
                    var (target, targetGroup) = tuple;

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
                        Debug.Log($"Could not set {string.Join(",", symbolsToKeep)} defines for build target: {target} group: {targetGroup} {e}");
                    }
                });
        }

        public static IEnumerable<string> GetDefinedSymbols(BuildTargetGroup group)
        {
            return PlayerSettings
                .GetScriptingDefineSymbolsForGroup(group)
                .Split(';')
                .Select(symbol => symbol.Trim())
                .Where(symbol => !string.IsNullOrEmpty(symbol));
        }
    }
}

#endif