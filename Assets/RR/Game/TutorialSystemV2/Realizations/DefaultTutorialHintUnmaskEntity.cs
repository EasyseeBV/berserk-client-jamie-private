using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Data;
using RR.Game.TutorialSystemV2.Data;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Realizations
{
    public class DefaultTutorialHintUnmaskEntity : ITutorialHintUnmaskEntity
    {
        private readonly RectTransform unmaskTransform;
        private readonly RectTransform parentTransform;
        private readonly ITutorialCameraProvider tutorialCamera;
        public ITutorialUnmaskData Data { get; }
        public ITutorialHintTarget Target { get; }

        public DefaultTutorialHintUnmaskEntity(
	        RectTransform unmask,
            ITutorialUnmaskData data, 
	        ITutorialCameraProvider tutorialCamera,
            ITutorialHintTarget target = null)
        {
            unmaskTransform = unmask;
            this.tutorialCamera = tutorialCamera;
            parentTransform = (RectTransform) unmaskTransform.parent;
            Data = data;
            Target = target;
            // default state is enabled
            unmaskTransform.gameObject.SetActive(true);
        }
        
        public virtual void Refresh()
        {
            Visibility();
            Reposition();
            Resize();
        }

        public virtual void Dispose()
        {
            if (unmaskTransform)
	            unmaskTransform.DestroyGameObject();
        }

        protected virtual void Visibility()
        {
            if (Target is not null || !unmaskTransform || !unmaskTransform.gameObject)
                return;
            
            unmaskTransform.gameObject.SetActive(!Data.HideIfMissTarget);
        }

        protected virtual void Reposition()
        {
            if (Data.DisableReposition)
                return;
            
            if (Data.FitTargetPosition)
            {
                if (Target != null)
                    RepositionInternal(Target.Position);
                return;
            }
            
            RepositionInternal(Data.Position);
        }

        protected virtual void Resize()
        {
            if (Data.DisableResize)
                return;
            
            if (Data.FitTargetSize)
            {
                if (Target != null)
                    ResizeInternal(Target.Size);
                return;
            }
            
            ResizeInternal(Data.Size);
        }

        protected virtual void ResizeInternal(TutorialVector3 value)
        {
            unmaskTransform.sizeDelta = value;
        }

        protected virtual void RepositionInternal(TutorialVector3 value)
        {
            if (Data.FitTargetPosition)
            {
                // from world space into anchored position
                var screenNormalizedPosition = tutorialCamera.Get().WorldToViewportPoint(value);
                var screenSize = parentTransform.rect.size;
                var anchoredPosition = (screenSize * 0.5f) - (screenNormalizedPosition * screenSize);
                unmaskTransform.anchoredPosition = -anchoredPosition;
                return;
            }

            // offset start from parents' rectangle center
            unmaskTransform.anchoredPosition = parentTransform.rect.center + value;
        }
    }
}