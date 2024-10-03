using System.Collections.Generic;
using System.Threading;
using BerserkV3.GameCore.Cards.EffectHints;
using BerserkV3.GameCore.TooltipPopup.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.GameCore.TooltipPopup
{
	public interface ITooltipPopupView
	{
		public UniTask InitAsync(List<TooltipEffectData> data, Transform parent, EffectOrigin origin, CancellationToken ctn);

		void Show();

		void Close();
	}
}