using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using UnityEngine.UI;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.ArrangeInShowAll)]
	public class ArrangeInShowVisual : EffectVisual
	{
		private readonly IGameContainers gameContainers;
		private const float DURATION = 2f;

		public ArrangeInShowVisual(IGameContainers gameContainers)
		{
			this.gameContainers = gameContainers;
		}

		public override async UniTask PlaySingleEffectAsync()
		{
			Targets.OfType<ICardView>().OrderBy(x=> x.RuntimeData.RelativePositionX).ForEach((x,i) => x.SelfContainer.SetSiblingIndex(i));
			LayoutRebuilder.ForceRebuildLayoutImmediate(gameContainers.InShowFirstRow);
			await UniTask.Delay(TimeSpan.FromSeconds(DURATION));
		}
	}
}