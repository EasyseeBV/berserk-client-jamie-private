using System;

namespace Berserk.Shared.GameCore.Attributes
{
    public class CmdRequiredTypeAttribute : Attribute
    {
        public Type Executor { get; set; }
        public Type Targets { get; set; }
    }
}
