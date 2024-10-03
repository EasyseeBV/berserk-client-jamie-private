using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RR.Core.ResourceManagament;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.UI
{
	public interface ILavaView
	{
		void Refresh(int currCount, int displayMax);
	}
	public class LavaView : BaseView, ILavaView
	{
		[SerializeField] private RawImage lavaGlow;
		[SerializeField] private RawImage foreGround;
		[SerializeField] private TextMeshProUGUI lavaValueText;
		[SerializeField] private Transform lavaItemsContainer;
		[SerializeField] private LavaItemView lavaItemPrefab;
		[SerializeField] private int maxLavaCount = 10;
		private LavaItemView[] lavaItemViews;
		private Tween glowTween;

		protected override void OnAwake()
		{
			base.OnAwake();
			lavaItemViews = new LavaItemView[maxLavaCount];
			
			if (maxLavaCount > 0 && (!lavaItemsContainer || !lavaItemPrefab))
				throw new NullReferenceException($"To create lava items need setup container and " +
				                                 $"{nameof(LavaItemView)} prefab in inspector");
			
			for (var i = 0; i < maxLavaCount; i++)
			{
				var item = Instantiate(lavaItemPrefab, lavaItemsContainer);
				item.SetActive(true);
				lavaItemViews[i] = item;
			}
			lavaGlow.LoadResourceAsync("VFX_Soft_Glowing").Forget();
			foreGround.LoadResourceAsync("Vulcanite_Attribute_Lava").Forget();
		}

		private void OnDestroy()
		{
			lavaGlow.ReleaseResource();
			foreGround.ReleaseResource();
		}

		public void Refresh(int currCount, int displayMax)
		{
			Set(lavaValueText, $"{currCount}/{displayMax}");

			if (lavaItemViews.Length > 0)
			{
				lavaItemViews.Skip(currCount).ForEach(x => x.FadeInAnimate());
				lavaItemViews.Take(currCount).ForEach(x => x.FadeOutAnimate());
			}
			
			AnimateGlow();
		}

		private void AnimateGlow()
		{
			glowTween?.Kill();
			glowTween = DOTween.Sequence()
				.Append(lavaGlow.DOFade(1f, 0.75f))
				.Append(lavaGlow.DOFade(0f, 0.75f))
				.SetEase(Ease.InOutCubic)
				.SetAutoKill(true)
				.Play();
		}
	}
}