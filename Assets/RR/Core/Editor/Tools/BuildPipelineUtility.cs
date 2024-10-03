using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace RR.Core.Editor.Tools
{
    public static class BuildPipelineUtility
    {
        public static IEnumerable<BuildTarget> GetAvailableBuildTargets()
        {
            var buildTargetType = typeof(BuildTarget);
            return Enum.GetNames(buildTargetType)
                .SelectMany(name => buildTargetType.GetMember(name))
                .Where(member => member.DeclaringType == buildTargetType)
                .Where(member => !member.IsDefined(typeof(ObsoleteAttribute), false))
                .Select(member => Enum.Parse(buildTargetType, member.Name, false))
                .Cast<BuildTarget>()
                .ToArray();
        }

        public static IEnumerable<BuildTargetGroup> GetAvailableBuildTargetGroups()
        {
            return GetAvailableBuildTargets()
                .Select(BuildPipeline.GetBuildTargetGroup)
                .Distinct();
        }
    }
}
