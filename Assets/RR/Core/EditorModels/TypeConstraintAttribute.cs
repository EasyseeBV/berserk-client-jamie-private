using System;
using UnityEngine;

namespace RR.Core.EditorModels
{
    public class TypeConstraintAttribute : PropertyAttribute
    {
        public Type Type { get; }

        public TypeConstraintAttribute(Type type)
        {
            Type = type;
        }
    }
}
