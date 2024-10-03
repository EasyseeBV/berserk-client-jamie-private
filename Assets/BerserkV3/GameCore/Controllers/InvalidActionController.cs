using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.UI;
using DG.Tweening;
using Sirenix.Utilities;
using Zenject;

namespace BerserkV3.GameCore.Controllers
{
	public interface IInvalidActionController
	{
		void OnInvalidAction(InvalidAction invalidAction);
		bool HandleInvalidAction(IRuntimeGameObject executor);
	}

	public class InvalidActionController : DisposableWithCts, IInvalidActionController, IInitializable
	{
		private const float STAY_VISIBLE_DURATION = 1.5f;
		private const float FADE_DURATION = 0.75f;
		
		private readonly InfoView infoView;
		private readonly IGameLogicContext gameLogicContext;
		private readonly IGameDatabase gameDatabase;
		private Sequence sequence;
		
		public InvalidActionController(
			InfoView infoView, 
			IGameLogicEventsSource gameLogicEventsSource,
			IGameLogicContext gameLogicContext, 
			IGameDatabase gameDatabase)
		{
			this.infoView = infoView;
			this.gameLogicContext = gameLogicContext;
			this.gameDatabase = gameDatabase;
			
			
			gameLogicEventsSource.Subscribe<InvalidActionEvent>(data => OnInvalidAction(data.Value), Token);
		}

		public void Initialize()
		{
			infoView.CanvasGroup.alpha = 0;
			infoView.gameObject.SetActive(true);

			gameLogicContext.TargetConditionRepository.OnInvalidAction += OnInvalidAction;
		}

		public override void Dispose()
		{
			base.Dispose();
			gameLogicContext.TargetConditionRepository.OnInvalidAction -= OnInvalidAction;
		}

		public void OnInvalidAction(InvalidAction invalidAction)
		{
			var text = gameDatabase.GetLocalization($"{nameof(InvalidAction)}.{invalidAction}");

			sequence?.Kill();
			sequence = DOTween.Sequence();

			sequence.AppendCallback(() => infoView.SetText(text));
			sequence.Append(infoView.CanvasGroup.DOFade(1, FADE_DURATION).From(0f));
			sequence.AppendInterval(STAY_VISIBLE_DURATION);
			sequence.Append(infoView.CanvasGroup.DOFade(0, FADE_DURATION));
			sequence.OnComplete(() => sequence = null);
			sequence.SetAutoKill(true);
			sequence.Play();
		}
		
		public bool HandleInvalidAction(IRuntimeGameObject executor)
		{
			if (executor?.RuntimeData == null 
			    || executor.RuntimeData.ImposingEffects.IsNullOrEmpty() 
			    || executor.Data.InvalidActions.IsNullOrEmpty())
				return false;

			foreach (var effectId in executor.RuntimeData.ImposingEffects)
			{
				foreach (var action in executor.Data.InvalidActions)
				{
					switch (action)
					{
						case InvalidAction.NoAvailableTargets:
						case InvalidAction.NoSpellTarget:
							if (!gameLogicContext.TargetConditionRepository.GetAllowedTargets(executor, effectId).IsNullOrEmpty())
								continue;

							break;
						
						case InvalidAction.TableIsFull :
							if (!gameLogicContext.TargetConditionRepository.IsFullTable(executor.RuntimeData.OwnerUserId))
								continue;
							
							break;
						// TODO implement others
						default: continue;
					}
					
					if (sequence == null)
						OnInvalidAction(action);
					return true;
				}	
			}

			return false;
		}
	}
}