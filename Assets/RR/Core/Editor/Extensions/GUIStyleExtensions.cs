using System;
using UnityEngine;

namespace RR.Core.Editor.Extensions
{
    public static class GUIStyleExtensions
    {
        public static GUIStyle Extend(this GUIStyle style, Action<GUIStyle> extender)
        {
            var newStyle = new GUIStyle(style);
            extender(newStyle);
            return newStyle;
        }
    }
}