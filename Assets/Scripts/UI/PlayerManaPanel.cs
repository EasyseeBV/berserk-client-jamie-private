using DG.Tweening;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public partial class PlayerManaPanel : BaseView
	{
		[SerializeField] private bool isOpponentManna;
		private List<Image> manaDots;
		private Tween glowTween;

		protected void Awake()
		{
			manaDots = isOpponentManna 
				? new List<Image>() 
				:ManaDotsPanel
				.GetChildren()
				.Select(x => x.GetChild(0).GetComponent<Image>())
				.ToList();
		}

		public void VisualizeMana(int mana, int max)
		{
			ManaValueTxt.SetText($"{Mathf.Max(0, mana)}/{max}");
			AnimateGlow();
			if(!isOpponentManna)
			{
				manaDots
				.Skip(mana)
				.ForEach(x =>
				{
					x.DOColor(Color.black, 0.75f);
					x.DOFade(0.2f, .75f);
				});
				
				manaDots
					.Take(mana)
					.ForEach(x =>
					{
						x.DOColor(Color.white, 0.75f);
						x.DOFade(1, .75f);
					});
			}
		}

		private void AnimateGlow()
		{
			if (glowTween != null) return;

			glowTween = LavaGlow.DOFade(1f, 0.75f)
								.SetEase(Ease.InOutCubic)
								.SetLoops(2, LoopType.Yoyo)
								.OnComplete(() => glowTween = null);
		}	
	}
}