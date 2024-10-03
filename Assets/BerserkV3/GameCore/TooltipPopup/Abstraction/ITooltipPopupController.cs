using System.Threading;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.GameCore.Cards.EffectHints;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.GameCore.TooltipPopup
{
	public interface ITooltipPopupController
	{
		public UniTask DisplayAsync(IRuntimeData runtimeData, EffectOrigin origin, Transform container, CancellationToken ctn);

		public void Close();
	}
}