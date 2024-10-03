using System;
using RR.Game.TutorialSystemV2.Abstraction.Data;

namespace RR.Game.TutorialSystemV2.Data
{
    [Serializable]
    public class TutorialUnmaskData : ITutorialUnmaskData
    {
        public virtual TutorialVector2 Position { get; set; }
        public virtual TutorialVector2 Size { get; set; }
        public virtual string Meta { get; set; } = string.Empty;
        public virtual bool HideIfMissTarget { get; set; } = true;
        public virtual bool DisableReposition { get; set; } = false;
        public virtual bool FitTargetPosition { get; set; } = false;
        public virtual bool DisableResize { get; set; } = false;
        public virtual bool FitTargetSize { get; set; } = false;
        public virtual bool AutoRefresh { get; set; } = false;
    }
}