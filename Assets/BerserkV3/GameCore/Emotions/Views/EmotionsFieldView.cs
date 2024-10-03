using Berserk.Shared.Data.Enums;
using DG.Tweening;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.GameCore.Emotions
{
	public class EmotionsFieldView : BaseView, IEmotionsFieldView
	{
		protected enum RearrangeDir
		{
			Left,
			Right
		}

		[SerializeField] protected float rearrangeSpacing = -60f;
		[SerializeField] protected float rearrangeDuration = 0.5f;
		[SerializeField] protected Ease rearrangeEase = Ease.InExpo;
		[SerializeField] protected RearrangeDir rearrangeDir;
		[SerializeField] protected Owner fieldOwner;

		private Tween rearragne;

		public Owner FieldOwner => fieldOwner;
		
		public RectTransform Container => RectTransform;

		public int Count => RectTransform.childCount;

		public void Rearrange()
		{
			rearragne?.Kill();
			var sequence = DOTween.Sequence();
			var childCount = Count - 1;
			var dir = GetRearrangeDir();
			var width = Container.rect.width;

			foreach (Transform child in Container)
			{
				if (!child)
				{
					childCount--;
					continue;
				}

				var offsetX = (childCount * width) + (childCount * rearrangeSpacing);
				sequence.Insert(0, child.DOLocalMove(offsetX * dir, rearrangeDuration));
				childCount--;
			}

			rearragne = sequence;
			sequence.SetEase(rearrangeEase);
			sequence.SetAutoKill(true);
			sequence.Play();
		}

		private Vector2 GetRearrangeDir()
		{
			return rearrangeDir == RearrangeDir.Left ? Vector2.left : Vector2.right;
		}

		private void Clear()
		{
			rearragne?.Kill();
			rearragne = null;
			if (Container)
				Container.DestroyChildren();
		}

		private void OnDestroy()
		{
			Clear();
		}
	}
}