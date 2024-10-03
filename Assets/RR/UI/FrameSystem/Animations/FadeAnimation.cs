using System;
using UnityEngine;

namespace RR.UI.FrameSystem.Animations
{
    [Serializable]
    public class FadeAnimation : BaseAnimation
    {
        public FadeAnimation()
        {
            Curve.AddKey(0, 0);
            Curve.AddKey(1, 1);
        }

        [NonSerialized]
        CanvasGroup cg;
        [NonSerialized]
        bool createdCg;

        public override void Init(RectTransform tr)
        {
            base.Init(tr);

            createdCg = false;
            cg = tr.GetComponent<CanvasGroup>();
            
            if (cg != null) 
                return;
            
            cg = tr.gameObject.AddComponent<CanvasGroup>();
            createdCg = true;
        }

        public override void OnFinish()
        {
            base.OnFinish();

            if (cg != null)
                cg.alpha = Mathf.RoundToInt(cg.alpha);

            if (createdCg)
                GameObject.Destroy(cg);
        }

        public override void Apply(RectTransform tr, float time)
        {
            if (cg)
            {
                var val = Curve.Evaluate(time);
                cg.alpha = val;
            }
        }
    }
}