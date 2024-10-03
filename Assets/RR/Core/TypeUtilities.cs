using System;
using System.Collections.Generic;
using System.Linq;

namespace RR.Core
{
    public static class TypeUtilities
    {   
        public static List<Type> GetAllTypesInProject()
        {
            List<Type> projectTypes = new List<Type>();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
                projectTypes.AddRange(assembly.IsDynamic ? assembly.GetTypes() : assembly.GetExportedTypes());
            
            return projectTypes;
        }

        public static Type FindTypeInProject(string typeName)
        {
            return GetAllTypesInProject().First(t => t.Name.Equals(typeName));
        }
    }
}