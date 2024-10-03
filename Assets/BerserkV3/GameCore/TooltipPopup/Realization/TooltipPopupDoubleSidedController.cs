using System.Threading;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.GameCore.Cards.EffectHints;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.GameCore.TooltipPopup
{
	public class TooltipPopupDoubleSidedController : ITooltipPopupDoubleSidedController
	{
		private readonly ITooltipPopupController gainedController;
		private readonly ITooltipPopupController innateController;
		
		private CancellationTokenSource displaySource;
		private CancellationToken token;
		
		private TooltipPopupDoubleSidedController(
			IGameDatabase gameDatabase,
			ITooltipPopupDoubleSidedView popup)
		{
			gainedController = new TooltipPopupController(gameDatabase, popup.GainedPopupView);
			innateController = new TooltipPopupController(gameDatabase, popup.InnatePopupView);
		}
		
		public UniTask DisplayAsync(IRuntimeData runtimeData, RectTransform container)
		{
			displaySource?.Cancel();
			displaySource?.Dispose();
			displaySource = new CancellationTokenSource();
			token = displaySource.Token;
			
			return UniTask.WhenAll(
				gainedController.DisplayAsync(runtimeData, EffectOrigin.Gained, container, token),
				innateController.DisplayAsync(runtimeData, EffectOrigin.Innate, container, token)
			).AttachExternalCancellation(token)
				.SuppressCancellationThrow();
		}

		public void Close()
		{
			displaySource?.Cancel();
			displaySource?.Dispose();
			displaySource = null;
			
			innateController.Close();
			gainedController.Close();
		}
	}
}