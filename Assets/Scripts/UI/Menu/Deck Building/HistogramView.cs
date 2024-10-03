using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Game;
using BerserkV3.Common.TutorialSystem;
using DG.Tweening;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

namespace UI
{
	public partial class HistogramView : BaseView
	{
		[SerializeField] private float fillHistogramOffset = 3;
		private readonly List<ProceduralImage> histogrammColumns = new();
		protected override void OnAwake()
		{
			base.OnAwake();
			RectTransform.SetHintTarget(TutorialTrigger.DeckLavaCurve.ToString()).SetTransitionFactorSize().Init();
		}

		public void Refresh(IReadOnlyCollection<CardData> deckCards)
		{
			SetHistogram();
			const int maxManna = 7;
			for (var i = 0; i < maxManna; i++)
			{
				var mana = i + 1;
				var manaCostCardsCount = deckCards.Count(x => mana == maxManna ? x.Mana >= mana : x.Mana == mana);
				var allCardsCount = (float) deckCards.Count;
				var columnValue = allCardsCount != 0
					? Math.Min(manaCostCardsCount / allCardsCount * fillHistogramOffset, 1f)
					: 0f;

				AnimateHistogram(i, columnValue);
			}
		}

		private void SetHistogram()
		{
			if (!histogrammColumns.IsNullOrEmpty())
				return;
			histogrammColumns.AddRange(new[]
			{
				Fill,
				Fill1,
				Fill2,
				Fill3,
				Fill4,
				Fill5,
				Fill6
			});
		}

		private void AnimateHistogram(int index, float value)
		{
			var fillImage = index < histogrammColumns.Count ? histogrammColumns[index] : null;
			if (fillImage == null)
				return;
				
			fillImage.DOKill();
			fillImage.DOFillAmount(value, 1f);
		}
	}
}