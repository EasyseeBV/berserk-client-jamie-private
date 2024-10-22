using DG.Tweening;
using RR.UIService.Data;
using UnityEngine;

namespace RR.UIService.AnimationSource
{
	[CreateAssetMenu(fileName = "UIFadeAnimation", menuName = "UIService/Animations/UIFadeAnimation")]
	public class UIFadeAnimationConfig : UIAnimationBaseConfig
	{
#if DOTWEEN
		[Header("===> DoTween <===")]
		[Space]
		[SerializeField] private Ease ease = Ease.Linear;
		[SerializeField] private bool useEase = true;        
		[Header("===> DoTween <===")]
		[Space]
#endif
		[SerializeField] protected float delay;
		[SerializeField] protected float duration = 1f;
		[SerializeField, Tooltip("If X (FromComponent) disabled it starts from current & if Y (ToComponent) it ends with current.")] 
		protected Vector2WithFlags fromTo;

#if DOTWEEN
		public bool UseEase => useEase;
		public Ease Ease => ease;
#endif
            
		public float Delay => delay;
		public float Duration => duration;
		public Vector2WithFlags FromTo => fromTo;
	}
}